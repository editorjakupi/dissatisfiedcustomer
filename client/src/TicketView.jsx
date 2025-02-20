import React, { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import "./TicketView.css"

function BoxesContainer() {
    const [ticketCounts, setTicketCounts] = useState({ active: 0, inactive: 0, resolved: 0, total: 0 });

    useEffect(() => {
        fetch("/api/tickets?view=all")
            .then((response) => response.json())
            .then((data) => {
                const activeCount = data.filter(ticket => ticket.status === "Unread" || ticket.status === "In Progress" || ticket.status === "Pending").length;
                const inactiveCount = data.filter(ticket => ticket.status === "Closed").length;
                const resolvedCount = data.filter(ticket => ticket.status === "Resolved").length;

                setTicketCounts({
                    active: activeCount,
                    inactive: inactiveCount,
                    resolved: resolvedCount,
                    total: data.length
                });
            })
            .catch((error) => console.error("Error fetching ticket counts:", error));
    }, []);

    return (
        <div className="boxes-container">
            <div className="column">
                <div className="box">Active: {ticketCounts.active}</div>
                <div className="box box2">Inactive: {ticketCounts.inactive}</div>
                <div className="box">Resolved: {ticketCounts.resolved}</div>
            </div>
            <div className="box box4">Total Tickets: {ticketCounts.total}</div>
        </div>
    );
}


// Regular TicketView
export default function TicketView() {
    const [tickets, setTickets] = useState([]);
    const [searchparams] = useSearchParams();
    const view = searchparams.get("view");
    
    useEffect(() => {
        fetch("/api/tickets?view=" + view)
            .then((response) => response.json())
            .then((data) => setTickets(data))
            .catch((error) => console.error("Error fetching tickets:", error));
    }, [view]);

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
        <>
        <div>
        <BoxesContainer />
        </div>
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
        </>
    );
};