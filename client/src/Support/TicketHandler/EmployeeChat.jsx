import React, { useState } from "react";
import "../../main.css";

const EmployeeChat = ({ ticket, fetchMessages }) => {
  const [messageContent, setMessageContent] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    if (ticket.status.toLowerCase() === "closed" || ticket.status.toLowerCase() === "resolved") {
      const alertMessage =
        ticket.status.toLowerCase() === "resolved"
          ? "Ticket is resolved. You cannot add new messages."
          : "Ticket is closed. You cannot add new messages.";
      alert(alertMessage);
      return;
    }

    const url = `http://localhost:5000/api/tickets/handle/${ticket.id}/messages`;
    console.log("POST URL (Employee):", url);
    fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ content: messageContent }),
      credentials: "include"
    })
      .then(response => {
        if (response.status === 204) {
          return {};
        }
        return response.json().catch(() => ({}));
      })
      .then(() => {
        setMessageContent("");
        fetchMessages(ticket.id);
      })
      .catch(error => {
        alert(error.message);
        console.error("Error adding message (Employee):", error);
      });
  };

  return (
    <form className="employee-chat-form" onSubmit={handleSubmit}>
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
  );
};

export default EmployeeChat;
