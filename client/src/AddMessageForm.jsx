import React, { useState } from 'react';
import './AddMessageForm.css';

const AddMessageForm = ({ userEmail, caseId, onMessageAdded, isSessionActive = true }) => {
    const [messageContent, setMessageContent] = useState("");

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!isSessionActive) {
            alert("Sessionen är avslutad. Du kan inte lägga till fler meddelanden.");
            return;
        }

        fetch(`http://localhost:5000/api/user/${userEmail}/cases/${caseId}/messages`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ email: userEmail, content: messageContent }),
            credentials: 'include',
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
            onMessageAdded(); // Uppdatera meddelandelistan efter att ett meddelande lagts till
        })
        .catch(error => {
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
                disabled={!isSessionActive}
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
