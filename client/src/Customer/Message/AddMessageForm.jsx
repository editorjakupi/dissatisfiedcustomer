import React, { useState, useEffect } from "react";
import "../../main.css"; // Ändra sökvägen om det behövs

const AddMessageForm = ({ userEmail, caseId, isSessionActive, ticketStatus }) => {
  const [messageContent, setMessageContent] = useState("");
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    // Poll for new messages every 5 seconds
    const intervalId = setInterval(() => {
      fetchMessages(caseId, userEmail);
    }, 5000);
    return () => clearInterval(intervalId);
  }, [caseId, userEmail]);

  useEffect(() => {
    // Log to verify the ticket status
    console.log("Ticket Status in Component:", ticketStatus);
  }, [ticketStatus]);

  const fetchMessages = (caseId, userEmail) => {
    fetch(`http://localhost:5000/api/user/${userEmail}/cases/${caseId}/messages`, {
      method: "GET",
      credentials: "include"
    })
      .then(response => {
        if (!response.ok) {
          return response.text().then(text => { throw new Error(text || "Error fetching messages"); });
        }
        return response.json();
      })
      .then(data => {
        console.log("Fetched messages:", data);
        setMessages(data);
      })
      .catch(err => {
        console.error("Error fetching messages:", err);
      });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    // Ange en alert om sessionen inte är aktiv (dvs ticketstatus=Closed/Resolved)
    if (!isSessionActive) {
      let alertMessage = "Ticket is closed. You cannot add new messages.";
      if (ticketStatus === "Resolved") {
        alertMessage = "Ticket is resolved. You cannot add new messages.";
      }
      alert(alertMessage);
      return;
    }

    fetch(`http://localhost:5000/api/user/${userEmail}/cases/${caseId}/messages`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email: userEmail, content: messageContent }),
      credentials: 'include'
    })
    .then(response => {
      if (!response.ok) {
        return response.text().then(text => { 
          throw new Error(text || "Error adding message"); 
        });
      }
      if (response.status === 204 || response.headers.get("content-length") === "0") {
        return {};
      }
      return response.json();
    })
    .then(() => {
      setMessageContent("");
      fetchMessages(caseId, userEmail); // Uppdatera meddelandelistan
    })
    .catch(error => {
      alert(error.message);
      console.error('Error adding message:', error);
    });
  };

  return (
    <div className="add-message-container">
      <ul className="messages-list">
        {messages && messages.length > 0 ? (
          messages.map((msg, index) => {
            const isCustomerMessage = (msg.email?.toLowerCase() === userEmail?.toLowerCase());
            const prefix = isCustomerMessage ? "Customer: " : "Customer Service: ";
            const messageClass = isCustomerMessage ? "customer-message" : "employee-message";
            return (
              <li key={index} className={`message-item ${messageClass}`}>
                <p>{prefix}{msg.content}</p>
              </li>
            );
          })
        ) : (
          <li className="message-item">No messages yet.</li>
        )}
      </ul>
      <form className="add-message-form" onSubmit={handleSubmit}>
        <textarea
          value={messageContent}
          onChange={(e) => setMessageContent(e.target.value)}
          placeholder="Enter your message"
          required
        />
        <div className="button-container">
          <button type="submit" className="add-message-button">
            Add Message
          </button>
        </div>
      </form>
    </div>
  );
};

export default AddMessageForm;
