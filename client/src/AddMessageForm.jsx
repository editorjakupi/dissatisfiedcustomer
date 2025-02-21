import React, { useState } from 'react';
import './AddMessageForm.css';

const AddMessageForm = ({ userId, caseId, onMessageAdded, onUpdateMessages }) => {
    const [messageContent, setMessageContent] = useState("");

    const handleSubmit = (e) => {
        e.preventDefault();

        fetch(`/api/user/${userId}/cases/${caseId}/messages`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ userId, content: messageContent }),
        })
        .then(response => response.json())
        .then(() => {
            setMessageContent("");
            onMessageAdded();
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
            />
            <div className="button-container">
                <button type="submit" className="add-message-button">Add Message</button>
                <button type="button" className="update-messages-button" onClick={onUpdateMessages}>Update Messages</button>
            </div>
        </form>
    );
};

export default AddMessageForm;
