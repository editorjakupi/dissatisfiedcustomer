import React, { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router';
import AddMessageForm from '../Message/AddMessageForm';
import SessionTimer from '../SessionTest&Timer/SessionTimer';
import '../../main.css';

const CustomerCases = () => {
  const { token } = useParams();
  const [ticket, setTicket] = useState(null);
  const [messages, setMessages] = useState([]);
  const [error, setError] = useState(null);
  const [isSessionActive, setIsSessionActive] = useState(true);
  const [sessionExpiry, setSessionExpiry] = useState(3600);
  const messagesEndRef = useRef(null);

  useEffect(() => {
    if (!token) return;
    // Hämta ticket med token
    fetch(`http://localhost:5000/api/tickets/view/${token}`, {
      method: 'GET',
      credentials: 'include',
      headers: { 'Cache-Control': 'no-cache' }
    })
      .then(response => {
        if (!response.ok)
          return response.text().then(text => { throw new Error(text || 'Ticket not found'); });
        return response.json();
      })
      .then(data => {
        console.log("Fetched ticket:", data);
        setTicket(data);
        const status = data.status ? data.status.toLowerCase().trim() : "";
        setIsSessionActive(status !== "closed" && status !== "resolved");
        if (data && data.id) {
          fetchMessages(data.id);
          startPolling(data.id); // Starta polling när ticket laddas
        } else {
          throw new Error("Ticket data is incomplete.");
        }
      })
      .catch(err => {
        console.error("Error fetching ticket:", err);
        setError(err.message);
      });
  }, [token]);

  // Polling för att hämta meddelanden kontinuerligt
  const startPolling = (ticketId) => {
    const intervalId = setInterval(() => {
      console.log("Polling messages for ticket:", ticketId);
      fetchMessages(ticketId);
    }, 5000);
    return () => clearInterval(intervalId);
  };

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const fetchMessages = (ticketId) => {
    console.log("Fetching messages for ticket:", ticketId);
    fetch(`/api/tickets/${ticketId}/messages`, {
      method: "GET",
      credentials: "include",
      headers: { "Cache-Control": "no-cache" }
    })
      .then(response => {
        if (!response.ok)
          return response.text().then(text => { throw new Error(text || "Error fetching messages"); });
        return response.json();
      })
      .then(data => {
        console.log("Fetched messages (raw):", data);
        setMessages(data);
      })
      .catch(err => {
        console.error("Error fetching messages:", err);
        setError(err.message);
      });
  };

  const handleMessageAdded = () => {
    if (ticket && ticket.id) {
      fetchMessages(ticket.id);
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
            const senderType = msg.senderType?.trim().toLowerCase() || "";
            const isEmployeeMessage = senderType === "employee";
            const prefix = isEmployeeMessage ? "Customer Service: " : "Customer: ";
            const messageClass = isEmployeeMessage ? "employee-message" : "customer-message";
            return (
              <li key={msg.messageId} className={`message-item ${messageClass}`}>
                <p>{prefix}{msg.content}</p>
              </li>
            );
          })
        ) : (
          <li className="message-item">No messages yet.</li>
        )}
        <div ref={messagesEndRef}></div>
      </ul>
      <h4>Add a Message</h4>
      <AddMessageForm 
        token={token}
        userEmail={ticket.email}
        onMessageAdded={handleMessageAdded}
        isSessionActive={isSessionActive}
        ticketStatus={ticket.status}
      />
    </div>
  );
};

export default CustomerCases;
