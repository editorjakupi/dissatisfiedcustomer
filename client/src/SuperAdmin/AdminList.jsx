import React, {useState} from "react";

const AdminList = ({ user, setUser }) => {
    const [message, setMessage] = useState("");

    const [Admin, setAdmin] = useState([]);
    const [selectedAdmin, setSelectedAdmin] = useState(null);
    const [searchId, setSearchId] = useState("");


    const handleSearch = () => {
        if (!searchId.trim()) return;

        fetch(`/api/adminlist/${searchId}`)
            .then((res) => {
                if (!res.ok) {
                    throw new Error(`Error: ${res.status} - ${res.statusText}`);
                }
                return res.json();
            })
            .then((data) => {
                console.log("Fetched Admin data:", data); // Log the fetched data
                if (data) {
                    setAdmin(data); // Set Admin to the single admin
                    setSelectedAdmin(data); // Set selected admin
                }
            })
            .catch((err) => {
                console.error("Search error:", err.message);
                setAdmin([]); // Clear Admin list if error
                setSelectedAdmin(null); // Clear selected admin
            });
    };


    // Show all Admin again
    const handleShowAll = () => {
        fetch(`/api/adminlist`)// Hård kodat in 3 för det är rollen för Admins
            .then((res) => res.json())
            .then((data) => {
                setAdmin(data); // Restore full list of Admin
                setSelectedAdmin(null); // Clear selected Admin
                setSearchId(""); // Reset search field
            })
            .catch((err) => console.error("Error fetching Admin:", err));
    };

    const handleDemote = () => {
        if (!selectedAdmin) return;

        fetch(`/api/adminlist/${selectedAdmin.id}`, {
            method: 'PUT',
        })
            .then((res) => {
                if (res.ok) {
                    setAdmin((prevAdmin) => prevAdmin.filter((admin) => admin.id !== selectedAdmin.userId));
                    setSelectedAdmin(null);
                } else {
                    console.error("Error demoting admin.");
                }
            })
            .catch((err) => {
                console.error("Delete demoting:", err);
            });
    };

    console.log("Admin state:", Admin); // Log the Admin state
    console.log("Selected Admin state:", selectedAdmin); // Log the selected admin state
    
    return (
        <main>
            <div className="user-container">
                {/* Search Bar */}
                <div className="search-container">
                    <input
                        type="text"
                        placeholder="Enter Company ID"
                        value={searchId}
                        onChange={(e) => setSearchId(e.target.value)}
                        className="search-input"
                    />
                    <div className="button-container">
                        <button onClick={handleSearch} className="search-button">Search</button>
                        <button onClick={handleShowAll} className="show-all-button">Show All</button>
                    </div>
                </div>

                <div className="main-container">
                    {/* admin List & Details */}
                    <div className="content-wrapper">
                        {/* admin List */}
                        <div className="users-list">
                            {Admin.length > 0 ? (
                                Admin.map((admin) => (
                                    <div
                                        key={admin.id}
                                        className="user-item"
                                        onClick={() => setSelectedAdmin(admin)}
                                    >
                                        {admin.name}
                                    </div>
                                ))
                            ) : (
                                <p>No Admin found.</p>
                            )}
                        </div>

                        {/* admin Details */}
                        <div className="user-details">
                            {selectedAdmin ? (
                                <div className="user-card">
                                    <h2>{selectedAdmin.name}</h2>
                                    <p>
                                        <strong>Name:</strong> {selectedAdmin.name}
                                    </p>
                                    <p>
                                        <strong>Phonenumber:</strong> {selectedAdmin.phonenumber}
                                    </p>
                                    <p>
                                        <strong>Company ID:</strong> {selectedAdmin.companyId}
                                    </p>
                                    <button onClick={handleDemote} className="demote-button">
                                        Demote admin
                                    </button>
                                </div>
                            ) : (
                                <p className="user-placeholder">Select a Admin to see details</p>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </main>
    )
};

export default AdminList;