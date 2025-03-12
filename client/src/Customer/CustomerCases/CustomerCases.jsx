// src/CustomerCases.jsx
import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router';
import AddMessageForm from '../../AddMessageForm';
import SessionTimer from '../../SessionTimer'; // Optional timer component
import '../../main.css';

const CustomerCases = () => {
  // Get the token from the URL (e.g., /tickets/view/CASE-XXXXXX)
  const { token } = useParams();

  const [ticket, setTicket] = useState(null);
  const [messages, setMessages] = useState([]);
  const [error, setError] = useState(null);
  const [isSessionActive, setIsSessionActive] = useState(true);
  const [sessionExpiry, setSessionExpiry] = useState(3600);

  // Fetch ticket details using the token
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
        // Set the session active flag based on ticket's status (case-insensitive)
        const status = data.status ? data.status.toLowerCase().trim() : "";
        setIsSessionActive(status !== "closed" && status !== "resolved");

        // Fetch messages if ticket data includes both id and email
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

  // Fetch messages based on ticketId and email.
  const fetchMessages = (ticketId, email) => {
    fetch(`http://localhost:5000/api/user/${email}/cases/${ticketId}/messages`, {
      method: "GET",
      credentials: 'include'
    })
      .then(response => {
        if (!response.ok) {
          return response.text().then(text => {
            throw new Error(text || "Error fetching messages");
          });
        }
        return response.json();
      })
      .then(data => setMessages(data))
      .catch(err => {
        console.error("Error fetching messages:", err);
        setError(err.message);
      });
  };

  // Callback for updating the message list
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
        {/* Display the current status of the ticket */}
        <p><strong>Status:</strong> {ticket.status}</p>
        <p><strong>Date:</strong> {new Date(ticket.date).toLocaleDateString()}</p>
      </div>

      <h4>Messages</h4>
      <ul className="messages-list">
        {messages && messages.length > 0 ? (
          messages.map((msg, index) => {
            // Determine if the message is from the customer (using ticket.email comparison)
            const isCustomerMessage = msg.email === ticket.email;
            const messageClass = isCustomerMessage ? "customer-message" : "support-message";
            return (
              <li key={index} className={`message-item ${messageClass}`}>
                <p>{msg.content}</p>
              </li>
            );
          })
        ) : (
          <li className="message-item">No messages yet.</li>
        )}
      </ul>

      <h4>Add a Message</h4>
      <AddMessageForm 
        userEmail={ticket.email} // Pass the ticket email
        caseId={ticket.id}
        onMessageAdded={handleMessageAdded}
        isSessionActive={isSessionActive}
      />
    </div>
  );
};

export default CustomerCases;
