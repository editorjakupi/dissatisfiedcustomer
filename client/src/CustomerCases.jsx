import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router';
import './CustomerCases.css';

const CustomerCases = ({ user }) => {
    const { userId } = useParams();
    const [cases, setCases] = useState([]);
    const [selectedCase, setSelectedCase] = useState(null);
    const [searchId, setSearchId] = useState("");

    useEffect(() => {
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
            })
            .catch((err) => {
                console.error("Search error:", err.message);
                setSelectedCase(null);
            });
    };

    const handleKeyDown = (e) => {
        if (e.key === 'Enter') {
            handleSearch();
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
                </div>
            )}
        </div>
    );
};

export default CustomerCases;
