import React, { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
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
    const navigate = useNavigate();
    const [sortedTickets, setSortedTickets] = useState([]);
    const [defaultOrder, setDefaultOrder] = useState([]);
    const [sortOrder, setSortOrder] = useState("default");
    const view = searchparams.get("view");
    
    useEffect(() => {
        fetch("/api/tickets?view=" + view)
            .then((response) => response.json())
            .then((data) => {
                setTickets(data);
                setSortedTickets(data);
                setDefaultOrder(data);
            })
            .catch((error) => console.error("Error fetching tickets:", error));
    }, [view]);

    function SortByTitle() {
        if (sortOrder === "default") {
            const sorted = [...sortedTickets].sort((a, b) => a.title.localeCompare(b.title));
            setSortedTickets(sorted);
            setSortOrder("asc");        
        } else if (sortOrder === "asc") {
            const sorted = [...sortedTickets].sort((a, b) => b.title.localeCompare(a.title));
            setSortedTickets(sorted);
            setSortOrder("desc");
        } else {
            setSortedTickets(defaultOrder);
            setSortOrder("default");
        }
    }

    // Rendering the table
    function TableItem(ticket) {
        return <tr key={"ticket-container-" + ticket.id}>
            <td>{ticket.date}</td>
            <td onClick={ () => navigate(`/tickets/handle/${ticket.id}`)}>{ticket.title}</td>
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
                        <th onClick={SortByTitle} style={{ cursor: "pointer" }}>
                            Title {sortOrder === "asc" ? "▲" : sortOrder === "desc" ? "▼" : ""}</th>
                        <th>Category</th>
                        <th>E-Mail</th>
                        <th>Status</th>
                        <th>Mark As Resolved</th>
                    </tr>
                </thead>
                <tbody>
                    {sortedTickets.map(ticket => <TableItem key={ticket.id} {...ticket} />)}
                </tbody>
            </table>
        </div>
        </>
    );
};