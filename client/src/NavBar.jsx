import React from "react";
import { useNavigate } from "react-router";
import "./NavBar.css";
import logo from "./assets/logo.png";

const NavBar = ({ user, setUser }) => {
    const navigate = useNavigate();
    
    const roleNames = {
        1: "Customer",
        2: "Employee",
        3: "Admin",
        4: "Super Admin"
    }
    
    const handleLogout = () => {
        localStorage.removeItem("user"); // Clear session
        setUser(null);
        navigate("/");
    }

    return (
        <nav className="sidebar">
            {/* Clickable Logo Section */}
            <div className="logo-container">
                <button onClick={() => navigate("/dashboard")} className="logo-button">
                    <img src={logo} alt="Dissatisfied Customer Logo" className="logo" />
                </button>
            </div>

            {/* Customer Panel (role_id 1) */}
            {user.role_id === 1 && (
                <div className="nav-section">
                    <h3>Customer Panel</h3>
                    <button onClick={() => navigate("/user/Tickets")}>My Tickets</button>
                    <button onClick={() => navigate("/user/account")}>My Account</button>
                </div>
            )}

            {/* Employee Panel (role_id 2) */}
            {user.role_id === 2 && (
                <div className="nav-section">
                    <h3>Employee Panel</h3>
                    <button onClick={() => navigate("/user/account")}>My Account</button>
                    <button onClick={() => navigate("/tickets")}>All Tickets</button>
                    <button onClick={() => navigate("/OpenTickets")}>Open Tickets</button>
                    <button onClick={() => navigate("/PendingTickets")}>Pending Tickets</button>
                    <button onClick={() => navigate("/ClosedTickets")}>Closed Tickets</button>
                </div>
            )}

            {/* Admin Panel (role_id 3) */}
            {user.role_id === 3 && (
                <div className="nav-section">
                    <h3>Admin Panel</h3>
                    <button onClick={() => navigate("/user/account")}>My Acccount</button>
                    <button onClick={() => navigate("/dashboard")}>Overview</button>
                    <button onClick={() => navigate("/products")}>Products</button>
                    <button onClick={() => navigate("/employees")}>Employees</button>
                </div>
            )}

            {/* Super Admin Panel (role_id 4) */}
            {user.role_id === 4 && (
                <div className="nav-section">
                    <h3>Super Admin Panel</h3>
                    <button onClick={() => navigate("/user/account")}>My Account</button>
                    <button onClick={() => navigate("/companies")}>Companies</button>
                    <button onClick={() => navigate("/admins")}>Admins</button>
                    <button onClick={() => navigate("/users")}>Users</button>
                </div>
            )}

            {/* Logout Button */}
            <div className="logout-container">
                <button className="logout-button" onClick={handleLogout}>
                    Logout
                </button>
            </div>

            <div className="user-info">
                <p><strong>{user?.name}</strong></p>
                <p>{roleNames[user?.role_id] || "Unknown Role"}</p>
            </div>
        </nav>
    );
};

export default NavBar;
