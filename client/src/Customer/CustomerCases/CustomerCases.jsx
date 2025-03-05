import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router';
import AddMessageForm from '../../AddMessageForm';
import './CustomerCases.css';

const CustomerCases = ({ user }) => {
    const { userId } = useParams();
    const [cases, setCases] = useState([]);
    const [selectedCase, setSelectedCase] = useState(null);
    const [messages, setMessages] = useState([]);
    const [searchId, setSearchId] = useState("");

    useEffect(() => {
        if (!userId) return; // Ensure userId is not undefined before making the request

        fetch(`/api/user/${userId}/cases`)
            .then(response => response.json())
            .then(data => {
                setCases(data);
            })
            .catch(error => {
                console.error('Error fetching cases:', error);
            });
    }, [userId]);


    const handleSearch = () => {
        if (!searchId.trim()) return;

        fetch(`/api/user/${userId}/cases/${searchId}`)
            .then((res) => {
                if (!res.ok) {
                    throw new Error(`Error: ${res.status} - ${res.statusText}`);
                }
                return res.json();
            })
            .then((data) => {
                setSelectedCase(data);
                fetchMessages(userId, data.caseId);
            })
            .catch((err) => {
                console.error("Search error:", err.message);
                setSelectedCase(null);
                setMessages([]);
            });
    };

    const fetchMessages = (userId, caseId) => {
        fetch(`/api/user/${userId}/cases/${caseId}/messages`)
            .then(response => response.json())
            .then(data => {
                setMessages(data);
            })
            .catch(error => {
                console.error('Error fetching messages:', error);
            });
    };

    const handleKeyDown = (e) => {
        if (e.key === 'Enter') {
            handleSearch();
        }
    };

    const handleMessageAdded = () => {
        // Uppdatera meddelanden efter att ett nytt meddelande har lagts till
        if (selectedCase) {
            fetchMessages(userId, selectedCase.caseId);
        }
    };

    const handleUpdateMessages = () => {
        // Hämta meddelanden på nytt
        if (selectedCase) {
            fetchMessages(userId, selectedCase.caseId);
        }
    };

    return (
        <div className="cases-container">
            <h2>My Cases</h2>
            <div className="search-container">
                <input
                    type="text"
                    placeholder="Enter Case ID"
                    value={searchId}
                    onChange={(e) => setSearchId(e.target.value)}
                    onKeyDown={handleKeyDown} // Lägg till hanteraren för keydown
                    className="search-input"
                />
                <button onClick={handleSearch} className="search-button">Search</button>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Case Number</th>
                        <th>Title</th>
                        <th>Description</th>
                        <th>Status</th>
                        <th>Date</th>
                    </tr>
                </thead>
                <tbody>
                    {cases.length > 0 ? (
                        cases.map((caseItem) => (
                            <tr key={caseItem.caseId}>
                                <td><Link to={`/user/${userId}/cases/${caseItem.caseId}`}>{caseItem.caseNumber}</Link></td>
                                <td>{caseItem.title}</td>
                                <td>{caseItem.description}</td>
                                <td>{caseItem.status}</td>
                                <td>{new Date(caseItem.createdAt).toLocaleDateString()}</td>
                            </tr>
                        ))
                    ) : (
                        <tr>
                            <td colSpan="5">No cases found for this user.</td>
                        </tr>
                    )}
                </tbody>
            </table>

            {selectedCase && (
                <div className="case-details">
                    <h3>Case Details</h3>
                    <p><strong>Case Number:</strong> {selectedCase.caseNumber}</p>
                    <p><strong>Title:</strong> {selectedCase.title}</p>
                    <p><strong>Description:</strong> {selectedCase.description}</p>
                    <p><strong>Status:</strong> {selectedCase.status}</p>
                    <p><strong>Date:</strong> {new Date(selectedCase.createdAt).toLocaleDateString()}</p>

                    <h4>Messages</h4>
                    <ul className="messages-list">
                        {messages.map((message, index) => (
                            <li key={index} className={`message-item ${message.userId === userId ? 'support-message' : 'customer-message'}`}>
                                <p>{message.content}</p>
                            </li>
                        ))}
                    </ul>

                    <h4>Add a Message</h4>
                    <AddMessageForm userId={userId} caseId={selectedCase.caseId} onMessageAdded={handleMessageAdded} onUpdateMessages={handleUpdateMessages} />
                </div>
            )}
        </div>
    );
};

export default CustomerCases;
