import React, { useState, useEffect } from 'react';
import '../../main.css';

const AddMessageForm = ({ userEmail, caseId, onMessageAdded, isSessionActive, ticketStatus }) => {
    const [messageContent, setMessageContent] = useState("");

    useEffect(() => {
        console.log("Ticket Status in Component:", ticketStatus);
    }, [ticketStatus]);

    const handleSubmit = (e) => {
        e.preventDefault();
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
            return response.text().then(text => text ? JSON.parse(text) : {});
        })
        .then(() => {
            setMessageContent("");
            onMessageAdded(); // Triggerar en uppdatering av meddelandelistan
        })
        .catch(error => {
            alert(error.message);
            console.error('Error adding message:', error);
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
                <button type="submit" className="add-message-button">
                    Add Message
                </button>
            </div>
        </form>
    );
};

export default AddMessageForm;
