import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter as Router, Routes, Route } from "react-router";
import Login from "./Login";
import Dashboard from "./dashboard";
import AccountInformation from './account.jsx'
import PasswordForget from './passwordforget.jsx'
import PasswordForget from './passwordforget.jsx'
import NavBar from "./NavBar";
import TicketView from "./TicketView";
import { OpenTicketView } from "./TicketView";
import { ClosedTicketView } from "./TicketView";
import { PendingTicketView } from "./TicketView";

import "./NavBar.css";
<<<<<<< HEAD
import ProductView from "./ProductView.jsx";
=======
import { Message } from "./message.jsx";
>>>>>>> main

const App = () => {
    const [user, setUser] = useState(() => {
        // Load user from localStorage if it exists
        const savedUser = localStorage.getItem("user");
        return savedUser ? JSON.parse(savedUser) : null;
    });

    useEffect(() => {
        if (user && user.role_id !== undefined) {
            localStorage.setItem("user", JSON.stringify(user)); // Save user, ensuring role_id is included
        } else {
            localStorage.removeItem("user"); // Clear storage if user is null
        }
    }, [user]);


    return (
        <Router>
            <div className="app-container">
                {user && <NavBar user={user} setUser={setUser} />} {/* Sidebar only shows when logged in */}

                <div className="content">

                    <Routes>
                        <Route path="/" element={<Login setUser={setUser} />} />
                        <Route path="/dashboard"
                            element={user ? <Dashboard user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/tickets" element={user ? <TicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/OpenTickets" element={user ? <OpenTicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/PendingTickets" element={user ? <PendingTicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/ClosedTickets" element={user ? <ClosedTicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path='/user/account' element={user ? <AccountInformation user={user} setUser={setUser} /> : <Login setUser={setUser} />} />
<<<<<<< HEAD
                        <Route path="/products" element={ <ProductView />} />
=======
                        <Route path="/forgot-password" element={<PasswordForget />} />
                        <Route path="/message/:id" element={<Message />} />
>>>>>>> main
import { OpenTicketView } from "./TicketView";
import { ClosedTicketView } from "./TicketView";
import { PendingTicketView } from "./TicketView";
import ProductView from "./ProductView.jsx";
import { Message } from "./message.jsx";
                        <Route path="/OpenTickets" element={user ? <OpenTicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/PendingTickets" element={user ? <PendingTicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/ClosedTickets" element={user ? <ClosedTicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/products" element={ <ProductView />} />
                        <Route path="/forgot-password" element={<PasswordForget />} />
                        <Route path="/message/:id" element={<Message />} />
                    </Routes>
                </div>
            </div>
        </Router>
    );
};

ReactDOM.createRoot(document.getElementById("root")).render(
    <React.StrictMode>
        <App />
    </React.StrictMode>
);