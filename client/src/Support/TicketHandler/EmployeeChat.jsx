import React, { useState, useEffect } from "react";

const EmployeeChat = ({ ticket, fetchMessages }) => {
  const [newMessage, setNewMessage] = useState("");

  // Poll for new messages every 5 seconds
  useEffect(() => {
    const intervalId = setInterval(() => {
      if (ticket && ticket.id && ticket.email) {
        fetchMessages(ticket.id, ticket.email);
      }
    }, 5000);
    return () => clearInterval(intervalId);
  }, [ticket, fetchMessages]);

  // Funktion för att skicka nytt meddelande från employee
  const handleSendMessage = () => {
    // Kontrollera att ärendet är aktivt
    const currentStatus = ticket.status ? ticket.status.toLowerCase().trim() : "";
    if (currentStatus === "closed" || currentStatus === "resolved") {
      alert(`Ticket is ${ticket.status}. You cannot add new messages.`);
      return;
    }

    // Använd en fast anställd e-post, t.ex. för kundservice
    const employeeEmail = "customerservice@company.com"; // Ändra vid behov

    fetch(`http://localhost:5000/api/user/${ticket.email}/cases/${ticket.id}/messages`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email: employeeEmail, content: newMessage }),
      credentials: "include"
    })
      .then(response => {
        if (!response.ok) {
          return response.text().then(text => {
            throw new Error(text || "Error adding message");
          });
        }
        if (response.status === 204 || response.headers.get("content-length") === "0") {
          return {}; // Ingen innehållssvar (204 No Content)
        }
        return response.json();
      })
      .then(() => {
        setNewMessage("");
        fetchMessages(ticket.id, ticket.email);
      })
      .catch(error => {
        alert(error.message);
        console.error("Error adding message:", error);
      });
  };

  return (
    <div className="employee-chat">
      <div className="chat-messages">
        {/* Här antar vi att dina meddelanden redan renderas någon annanstans i TicketHandler.jsx */}
      </div>
      <div className="chat-response-div">
        <textarea
          value={newMessage}
          onChange={(e) => setNewMessage(e.target.value)}
          placeholder="Type your message..."
          disabled={ticket.status && (ticket.status.toLowerCase().trim() === "closed" || ticket.status.toLowerCase().trim() === "resolved")}
        ></textarea>
        <button
          onClick={handleSendMessage}
          disabled={ticket.status && (ticket.status.toLowerCase().trim() === "closed" || ticket.status.toLowerCase().trim() === "resolved")}
        >
          Send
        </button>
      </div>
    </div>
  );
};

export default EmployeeChat;
