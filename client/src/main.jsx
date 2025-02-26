import React, {useState, useEffect, use} from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter as Router, Routes, Route } from "react-router";
import Login from "./Login";
import Dashboard from "./dashboard";
import AccountInformation from './account.jsx'
import PasswordForget from './passwordforget.jsx'
import NavBar from "./NavBar";
import TicketView from "./TicketView";
import UsersList from "./UsersList";
import { TicketForm } from "./TicketForm.jsx";
import NewEmployee from "./NewEmployee.jsx";

import "./NavBar.css";
import { Message } from "./message.jsx";
import NewProduct from "./NewProduct.jsx";
import CustomerCases from "./CustomerCases";

const App = () => {
    const [user, setUser] = useState(null);

    // Fetch session user when the app loads
    useEffect(() => {
        const fetchSessionUser = async () => {
            try {
                const response = await fetch("/api/session", {
                    credentials: "include", // Ensures cookies are sent
                });

                if (!response.ok) throw new Error("No session found");

                const data = await response.json();
                console.log("Session user data:", data);
                setUser(data);
            } catch {
                setUser(null); // No session available
            }
        };

        fetchSessionUser();
    }, []);


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
                        <Route path="/users" element={user ? <UsersList user={user}/> : <Login setUser={setUser}/> }/>
                        <Route path="/employee" element={user ? <NewEmployee user={user}/> : <Login setUser={setUser}/>}/>
                        <Route path="/products" element={user ? <NewProduct user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/user/:userId/cases" element={<CustomerCases user={user} />} />
                        <Route path="/user/:userId/cases/:caseId" element={<CustomerCases user={user} />} />
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