// src/CustomerCases.jsx
import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router';
import AddMessageForm from '../../AddMessageForm';
import SessionTimer from '../../SessionTimer'; // Om du valt att använda en timer
import './CustomerCases.css';

const CustomerCases = () => {
  // Hämtar token från URL:en (t.ex. /tickets/view/CASE-XXXXX)
  const { token } = useParams();

  const [ticket, setTicket] = useState(null);
  const [messages, setMessages] = useState([]);
  const [error, setError] = useState(null);
  const [isSessionActive, setIsSessionActive] = useState(true);
  // Spara sessionstiden i sekunder (60 minuter = 3600 sekunder)
  const [sessionExpiry, setSessionExpiry] = useState(3600);

  // Hämtar ärendedetaljer baserat på token
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
        setTicket(data);
        // Om status är "Closed" eller "Resolved" inaktiveras sessionen
        setIsSessionActive(data.status !== "Closed" && data.status !== "Resolved");
        // Om data finns och har både id och email, hämta meddelanden
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

  // Funktion för att hämta meddelanden baserat på ticketId och email
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

  // Callback för att uppdatera meddelandelistan
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
          messages.map((msg, index) => {
            // Avgör om meddelandet är från kunden (om msg.email matchar ticket.email)
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
        userEmail={ticket.email}  // Skicka med e-post från ärendet
        caseId={ticket.id} 
        onMessageAdded={handleMessageAdded} 
        isSessionActive={isSessionActive}
      />
    </div>
  );
};

export default CustomerCases;
