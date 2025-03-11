import React, { useState, useEffect } from "react";
import "../../main.css"; // Make sure this CSS file is correctly linked

const UsersList = () => {
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const [searchId, setSearchId] = useState("");

    const [formData, setFormData] = useState({
        name: "",
        email: "",
        password: "",
        phonenumber: "",
        companyId: "",
    });

    const [message, setMessage] = useState("");

    const handleChange = (e) => {
        setFormData({...formData, [e.target.name]: e.target.value});
    };
    
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
                    alert("User deleted successfully!");
                } else {
                    console.error("Error deleting user.");
                }
            })
            .catch((err) => {
                console.error("Delete error:", err);
            });
    };
    
    const handlePromote = async () => {
        if (!selectedUser) return;

        fetch(`/api/promoteuser/${selectedUser.id}`, {
            method: 'PUT',
        })
            .then((res) => {
                if (res.ok) {
                    setUsers((prevUser) => prevUser.filter((user) => user.id !== selectedUser.userId));
                    setSelectedUser(null);
                    alert("User promoted successfully!");
                } else {
                    console.error("Error promoting admin.");
                }
            })
            .catch((err) => {
                console.error("Delete promoting:", err);
            });
    };
    
    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage("");

        if (selectedUser) {
            // Update existing employee
            try {
                const response = await fetch(`/api/putuser/${selectedUser.id}`, {
                    method: "PUT",
                    headers: {"Content-Type": "application/json"},
                    body: JSON.stringify({
                        id: selectedUser.id,
                        name: formData.name,
                        email: formData.email,
                        phonenumber: formData.phonenumber,
                    }),
                });

                if (!response.ok) throw new Error("Failed to update User");

                setMessage("User updated successfully");
                setUsers((prev) => prev.map(emp => emp.id === selectedUser.id ? {...emp, ...formData} : emp));
            } catch (error) {
                setMessage(error.message);
            }
        } else {
            try {
                // Create the user
                const userResponse = await fetch("/api/users", {
                    method: "POST",
                    headers: {"Content-Type": "application/json"},
                    body: JSON.stringify({
                        name: formData.name,
                        email: formData.email,
                        password: formData.password,
                        phonenumber: formData.phonenumber,
                    }),
                });

                if (!userResponse.ok) throw new Error("Failed to create user");

                setMessage("User created successfully!");
            } catch (error) {
                console.error(error);
                setMessage(error.message);
            }
        }
    };
    const handleClearSelection = () => {
        setSelectedUser(null); // Reset selected employee
        setFormData({
            name: "",
            email: "",
            password: "",
            phonenumber: "",
            companyId: "",
        });
    };
    
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
                                onClick={() => {
                                    setSelectedUser(user);
                                    setFormData(user);
                                }}
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
                            <p>
                                <strong>Role ID:</strong> {selectedUser.role_id}
                            </p>
                            <button onClick={handleDelete} className="delete-button">
                                Delete User
                            </button>
                            <button onClick={handlePromote} className="promote-button">
                                Promote User
                            </button>
                            <button onClick={handleClearSelection} className="clear-button">
                                Clear Selection
                            </button>
                        </div>
                    ) : (
                        <p className="user-placeholder">Select a user to see details</p>
                    )}
                </div>

                <div className="form-container">
                    <form onSubmit={handleSubmit} className="form">
                        <h2>{selectedUser ? "Update User" : "Create New User"}</h2>
                        <input type="text" name="name" value={formData.name || ""} placeholder="Name"
                               onChange={handleChange} required/>
                        <input type="email" name="email" value={formData.email || ""} placeholder="Email"
                               onChange={handleChange} required/>
                        <input type="text" name="phonenumber" value={formData.phonenumber || ""}
                               placeholder="Phone Number"
                               onChange={handleChange} required/>
                        <button
                            type="submit">{selectedUser ? "Update User" : "Create User"}</button>
                    </form>
                    {message && <p>{message}</p>}
                </div>
            </div>
        </div>
    );
};

export default UsersList;
