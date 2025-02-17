import React, { useState } from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter as Router, Routes, Route } from "react-router";
import Login from "./Login";
import Dashboard from "./dashboard";
import AccountInformation from './account.jsx'
import { Message } from "./message.jsx";

const App = () => {
    const [user, setUser] = useState(null);

    return (
        <Router>
            <Routes>
                <Route path="/" element={<Login setUser={setUser} />} />
                <Route path="/dashboard" element={user ? <Dashboard user={user} /> : <Login setUser={setUser} />} />
                <Route path='/account' element={ <AccountInformation/> } />
                <Route path="/message/:id" element={<Message/>}/>
            </Routes>
        </Router>
    );
};

ReactDOM.createRoot(document.getElementById("root")).render(
    <React.StrictMode>
        <App />
    </React.StrictMode>
);