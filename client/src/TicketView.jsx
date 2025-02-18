import React, { useEffect, useState } from "react";
import "./TicketView.css"

// Regular TicketView
export default function TicketView() {
    const [tickets, setTickets] = useState([]);
    useEffect(() => {
        fetch("/api/tickets")
            .then((response) => response.json())
            .then((data) => {
                setTickets(data);
            })
            .catch((error) => console.error("Error fetching tickets:", error));
    }, []);

    // Rendering the table
    function TableItem(ticket) {
        return <tr key={"ticket-container-" + ticket.id}>
            <td>{ticket.date}</td>
            <td>{ticket.title}</td>
            <td>{ticket.categoryname}</td>
            <td>{ticket.email}</td>
            <td>{ticket.status}</td>
            <td><button className="resolve-button">Resolve Ticket</button></td>
        </tr>
    }
    return (
        <div className="ticket-container">
            <table>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Title</th>
                        <th>Category</th>
                        <th>E-Mail</th>
                        <th>Status</th>
                        <th>Mark As Resolved</th>
                    </tr>
                </thead>
                <tbody>
                    {tickets.map(TableItem)}
                </tbody>
            </table>
        </div>
    );
};



// Open TicketView
export function OpenTicketView() {
    const [tickets, setTickets] = useState([]);
    useEffect(() => {
        fetch("/api/tickets")
            .then((response) => response.json())
            .then((data) => {
                const filteredTickets = data.filter(ticket =>
                    ticket.status === "Unread" ||
                    ticket.status === "In Progress"
                );
                setTickets(filteredTickets);
            })
            .catch((error) => console.error("Error fetching tickets:", error));
    }, []);

    // Rendering the table
    function TableItem(ticket) {
        return <tr key={"ticket-container-" + ticket.id}>
            <td>{ticket.date}</td>
            <td>{ticket.title}</td>
            <td>{ticket.categoryname}</td>
            <td>{ticket.email}</td>
            <td>{ticket.status}</td>
            <td><button className="resolve-button">Resolve Ticket</button></td>
        </tr>
    }
    return (
        <div className="ticket-container">
            <table>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Title</th>
                        <th>Category</th>
                        <th>E-Mail</th>
                        <th>Status</th>
                        <th>Mark As Resolved</th>
                    </tr>
                </thead>
                <tbody>
                    {tickets.map(TableItem)}
                </tbody>
            </table>
        </div>
    );
};



// Inactive/Closed TicketView
export function ClosedTicketView() {
    const [tickets, setTickets] = useState([]);
    useEffect(() => {
        fetch("/api/tickets")
            .then((response) => response.json())
            .then((data) => {
                const filteredTickets = data.filter(ticket =>
                    ticket.status === "Closed" ||
                    ticket.status === "Resolved"
                );
                setTickets(filteredTickets);
            })
            .catch((error) => console.error("Error fetching tickets:", error));
    }, []);

    // Rendering the table
    function TableItem(ticket) {
        return <tr key={"ticket-container-" + ticket.id}>
            <td>{ticket.date}</td>
            <td>{ticket.title}</td>
            <td>{ticket.categoryname}</td>
            <td>{ticket.email}</td>
            <td>{ticket.status}</td>
            <td><button className="resolve-button">Resolve Ticket</button></td>
        </tr>
    }
    return (
        <div className="ticket-container">
            <table>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Title</th>
                        <th>Category</th>
                        <th>E-Mail</th>
                        <th>Status</th>
                        <th>Mark As Resolved</th>
                    </tr>
                </thead>
                <tbody>
                    {tickets.map(TableItem)}
                </tbody>
            </table>
        </div>
    );
};


// Pending Ticketview
export function PendingTicketView() {
    const [tickets, setTickets] = useState([]);
    useEffect(() => {
        fetch("/api/tickets")
            .then((response) => response.json())
            .then((data) => {
                const filteredTickets = data.filter(ticket =>
                    ticket.status === "Pending"
                );
                setTickets(filteredTickets);
            })
            .catch((error) => console.error("Error fetching tickets:", error));
    }, []);

    // Rendering the table
    function TableItem(ticket) {
        return <tr key={"ticket-container-" + ticket.id}>
            <td>{ticket.date}</td>
            <td>{ticket.title}</td>
            <td>{ticket.categoryname}</td>
            <td>{ticket.email}</td>
            <td>{ticket.status}</td>
            <td><button className="resolve-button">Resolve Ticket</button></td>
        </tr>
    }
    return (
        <div className="ticket-container">
            <table>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Title</th>
                        <th>Category</th>
                        <th>E-Mail</th>
                        <th>Status</th>
                        <th>Mark As Resolved</th>
                    </tr>
                </thead>
                <tbody>
                    {tickets.map(TableItem)}
                </tbody>
            </table>
        </div>
    );
};