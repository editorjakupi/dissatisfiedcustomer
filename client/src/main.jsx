import React, { useState, useEffect, use } from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter as Router, Routes, Route } from "react-router";
import Login from "./User/Login/Login.jsx";
import Dashboard from "./User/Dashboard/dashboard.jsx";
import AccountInformation from './User/Account/account.jsx'
import PasswordForget from './User/passwordforget.jsx'
import NavBar from "./User/NavBar/NavBar.jsx";
import TicketView from "./Support/TicketView/TicketView.jsx";
import UsersList from "./UsersList";
import { TicketForm } from "./Customer/TicketForm/TicketForm.jsx";
import NewEmployee from "./NewEmployee.jsx";

import "./User/NavBar/NavBar.css";
import { Message } from "./Customer/Message/message.jsx";
import TicketHandler from "./Support/TicketHandler/TicketHandler.jsx";
import NewProduct from "./NewProduct.jsx";
import CustomerCases from "./Customer/CustomerCases/CustomerCases";

const App = () => {
    const [user, setUser] = useState(null);


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
                        <Route path='/account' element={user ? <AccountInformation user={user} setUser={setUser} /> : <Login setUser={setUser} />} />
                        <Route path="/forgot-password" element={<PasswordForget />} />
                        <Route path="/message/:id" element={<Message />} />
                        <Route path="/ticketform/:caseNr" element={<TicketForm />} />
                        <Route path="/users" element={user ? <UsersList user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/employee" element={user ? <NewEmployee user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/products" element={user ? <NewProduct user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/user/:userId/cases" element={<CustomerCases user={user} />} />
                        <Route path="/user/:userId/cases/:caseId" element={<CustomerCases user={user} />} />
                        <Route path="/tickets/handle/:ticketId" element={<TicketHandler />} />
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