import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router';
import AddMessageForm from '../Message/AddMessageForm';
import SessionTimer from '../SessionTest&Timer/SessionTimer';
import '../../main.css';

const EMPLOYEE_EMAIL = "customerservice@company.com"; // Den fasta emailen för kundtjänst

const CustomerCases = () => {
  const { token } = useParams();
  const [ticket, setTicket] = useState(null);
  const [messages, setMessages] = useState([]);
  const [error, setError] = useState(null);
  const [isSessionActive, setIsSessionActive] = useState(true);
  const [sessionExpiry, setSessionExpiry] = useState(3600);
  const [pollingStarted, setPollingStarted] = useState(false);

  useEffect(() => {
    if (!token) return;

    fetch(`http://localhost:5000/api/tickets/view/${token}`, {
      method: "GET",
      credentials: 'include'
    })
      .then(response => {
        if (!response.ok) {
          return response.text().then(text => {
            throw new Error(text || "Ticket not found");
          });
        }
        return response.json();
      })
      .then(data => {
        console.log("Fetched ticket status:", data.status);
        setTicket(data);
        const status = data.status ? data.status.toLowerCase().trim() : "";
        setIsSessionActive(status !== "closed" && status !== "resolved");

        if (data && data.id && data.email) {
          fetchMessages(data.id, data.email);
        } else {
          throw new Error("Ticket data is incomplete.");
        }
      })
      .catch(err => {
        console.error("Error fetching ticket details:", err);
        setError(err.message);
      });
  }, [token]);

  useEffect(() => {
    if (ticket && ticket.id && ticket.email && !pollingStarted) {
      const intervalId = setInterval(() => {
        console.log("Polling messages for ticket:", ticket.id);
        fetchMessages(ticket.id, ticket.email);
      }, 5000);
      setPollingStarted(true);
      return () => clearInterval(intervalId);
    }
  }, [ticket, pollingStarted]);

  const fetchMessages = (ticketId, email) => {
    console.log('Fetching messages for ticket:', ticketId);
    fetch(`http://localhost:5000/api/user/${email}/cases/${ticketId}/messages`, {
      method: "GET",
      credentials: 'include'
    })
      .then(response => {
        if (!response.ok) {
          return response.text().then(text => { throw new Error(text || "Error fetching messages"); });
        }
        return response.json();
      })
      .then(data => {
        console.log('Fetched messages (raw):', data);
        const deduped = data.reduce((acc, current) => {
          const existing = acc.find(item => item.id === current.id);
          if (!existing) {
            acc.push(current);
          }
          return acc;
        }, []);
        console.log('Deduplicated messages:', deduped);
        setMessages(deduped);
      })
      .catch(err => {
        console.error("Error fetching messages:", err);
        setError(err.message);
      });
  };

  const handleMessageAdded = () => {
    if (ticket && ticket.id && ticket.email) {
      fetchMessages(ticket.id, ticket.email);
    }
  };

  if (error) {
    return <p className="error">Error: {error}</p>;
  }

  if (!ticket) {
    return <p>Loading chat session...</p>;
  }

  return (
    <div className="cases-container">
      <h2>Chat Session</h2>
      {isSessionActive && <SessionTimer durationInSeconds={sessionExpiry} />}
      
      <div className="case-details">
        <p><strong>Case Number:</strong> {ticket.caseNumber}</p>
        <p><strong>Title:</strong> {ticket.title}</p>
        <p><strong>Description:</strong> {ticket.description}</p>
        <p><strong>Status:</strong> {ticket.status}</p>
        <p><strong>Date:</strong> {new Date(ticket.date).toLocaleDateString()}</p>
      </div>

      <h4>Messages</h4>
      <ul className="messages-list">
        {messages && messages.length > 0 ? (
          messages.map((msg) => {
            const msgEmail = msg.email?.trim().toLowerCase();
            console.log("Medelandets email:", msgEmail);
            const isCustomerMessage = (msgEmail !== EMPLOYEE_EMAIL);
            const prefix = isCustomerMessage ? "Customer: " : "Customer Service: ";
            const messageClass = isCustomerMessage ? "customer-message" : "employee-message";
            return (
              <li key={msg.id} className={`message-item ${messageClass}`}>
                <p>{prefix}{msg.content}</p>
              </li>
            );
          })
        ) : (
          <li className="message-item">No messages yet.</li>
        )}
      </ul>

      <h4>Add a Message</h4>
      <AddMessageForm 
        userEmail={ticket.email}
        caseId={ticket.id}
        onMessageAdded={handleMessageAdded}
        isSessionActive={isSessionActive}
        ticketStatus={ticket.status}
      />
    </div>
  );
};

export default CustomerCases;
