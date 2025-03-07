import React, { useState, useEffect } from "react";
import "../../main.css"; // Make sure this CSS file is correctly linked

const UsersList = () => {
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const [searchId, setSearchId] = useState("");

    // Fetch all users on component mount
    useEffect(() => {
        fetch("/api/users")
            .then((res) => res.json())
            .then((data) => setUsers(data))
            .catch((err) => console.error("Error fetching users:", err));
    }, []);
    

    // Search user by ID
    const handleSearch = () => {
        if (!searchId.trim()) return;

        fetch(`/api/users/${searchId}`)
            .then((res) => {
                if (!res.ok) {
                    throw new Error(`Error: ${res.status} - ${res.statusText}`);
                }
                return res.json();
            })
            .then((data) => {
                console.log("Fetched user data:", data); // Log the fetched data
                if (data) {
                    setUsers(data); // Set users to the single user
                    setSelectedUser(data); // Set selected user
                }
            })
            .catch((err) => {
                console.error("Search error:", err.message);
                setUsers([]); // Clear users list if error
                setSelectedUser(null); // Clear selected user
            });
    };


    // Show all users again
    const handleShowAll = () => {
        fetch("/api/users")
            .then((res) => res.json())
            .then((data) => {
                setUsers(data); // Restore full list of users
                setSelectedUser(null); // Clear selected user
                setSearchId(""); // Reset search field
            })
            .catch((err) => console.error("Error fetching users:", err));
    };

    const handleDelete = () => {
        if (!selectedUser) return;

        fetch(`/api/users/${selectedUser.id}`, {
            method: 'DELETE',
        })
            .then((res) => {
                if (res.ok) {
                    setUsers((prevUsers) => prevUsers.filter((user) => user.id !== selectedUser.id));
                    setSelectedUser(null); 
                } else {
                    console.error("Error deleting user.");
                }
            })
            .catch((err) => {
                console.error("Delete error:", err);
            });
    };
    
    console.log("Users state:", users); // Log the users state
    console.log("Selected User state:", selectedUser); // Log the selected user state

    return (
        <div className="users-container">
            {/* Search Bar */}
            <div className="search-container">
                <input
                    type="text"
                    placeholder="Enter user ID"
                    value={searchId}
                    onChange={(e) => setSearchId(e.target.value)}
                    className="search-input"
                />
                <button onClick={handleSearch} className="search-button">
                    Search
                </button>
                <button onClick={handleShowAll} className="show-all-button">
                    Show All
                </button>
            </div>

            {/* User List & Details */}
            <div className="content-wrapper">
                {/* User List */}
                <div className="users-list">
                    {users.length > 0 ? (
                        users.map((user) => (
                            <div
                                key={user.id}
                                className="user-item"
                                onClick={() => setSelectedUser(user)}
                            >
                                {user.name}
                            </div>
                        ))
                    ) : (
                        <p>No users found.</p>
                    )}
                </div>

                {/* User Details */}
                <div className="user-details">
                    {selectedUser ? (
                        <div className="user-card">
                            <h2>{selectedUser.name}</h2>
                            <p>
                                <strong>Email:</strong> {selectedUser.email}
                            </p>
                            <p>
                                <strong>Phone:</strong> {selectedUser.phonenumber}
                            </p>
                            <button onClick={handleDelete} className="delete-button">
                                Delete User
                            </button>
                        </div>
                    ) : (
                        <p className="user-placeholder">Select a user to see details</p>
                    )}
                </div>
            </div>
        </div>
    );
};

export default UsersList;
