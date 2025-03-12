import React, { useState, useEffect } from "react";
import "../../main.css";

const AddMessageForm = ({ token, userEmail, onMessageAdded, isSessionActive, ticketStatus }) => {
  const [messageContent, setMessageContent] = useState("");

  useEffect(() => {
    console.log("Ticket Status in Component:", ticketStatus);
  }, [ticketStatus]);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!isSessionActive) {
      const alertMessage =
        ticketStatus === "Resolved"
          ? "Ticket is resolved. You cannot add new messages."
          : "Ticket is closed. You cannot add new messages.";
      alert(alertMessage);
      return;
    }

    const url = `http://localhost:5000/api/tickets/view/${token}/messages`;
    console.log("POST URL (Customer):", url);
    fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ email: userEmail, content: messageContent }),
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
        onMessageAdded();
      })
      .catch(error => {
        alert(error.message);
        console.error("Error adding message (Customer):", error);
      });
  };

  return (
    <form className="add-message-form" onSubmit={handleSubmit}>
      <textarea
        value={messageContent}
        onChange={(e) => setMessageContent(e.target.value)}
        placeholder="Enter your message"
        required
      />
      <div className="button-container">
        <button type="submit" className="add-message-button" disabled={!isSessionActive}>
          Add Message
        </button>
      </div>
    </form>
  );
};

export default AddMessageForm;
