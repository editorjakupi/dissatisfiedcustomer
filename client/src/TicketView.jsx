import React, { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
import "./main.css"

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
    
    const [sortOrderTitle, setSortOrderTitle] = useState("default");
    const [sortOrderCategory, setSortOrderCategory] = useState("default");

    const view = searchparams.get("view");
    
    // Sort by title function
    function SortByTitle() {
        let sorted;

        if (sortOrderTitle === "default") {
            sorted = [...tickets].sort((a, b) => a.title.localeCompare(b.title, "sv"));
            setSortOrderTitle("asc");
        } else if (sortOrderTitle === "asc") {
            sorted = [...tickets].sort((a, b) => b.title.localeCompare(a.title, "sv"));
            setSortOrderTitle("desc");
        } else {
            sorted = [...defaultOrder];
            setSortOrderTitle("default");
        }

        // Reset category sorting state
        setSortOrderCategory("default");

        setSortedTickets(sorted);
    }

    function SortByCategory() {
        let sorted;

        if (sortOrderCategory === "default") {
            sorted = [...tickets].sort((a, b) => a.categoryname.localeCompare(b.categoryname, "sv"));
            setSortOrderCategory("asc");
        } else if (sortOrderCategory === "asc") {
            sorted = [...tickets].sort((a, b) => b.categoryname.localeCompare(a.categoryname, "sv"));
            setSortOrderCategory("desc");
        } else {
            sorted = [...defaultOrder];
            setSortOrderCategory("default");
        }

        // Reset title sorting state
        setSortOrderTitle("default");

        setSortedTickets(sorted);
    }
    
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
        <main id="ticket-view-main">
            <div className="info-boxes-div">
                <BoxesContainer />
            </div>
            <div className="ticket-container">
                <table>
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th onClick={SortByTitle} style={{ cursor: "pointer" }}>
                                Title {sortOrderTitle === "asc" ? "▲" : sortOrderTitle === "desc" ? "▼" : ""}
                                </th>
                            <th onClick={SortByCategory} style={{ cursor: "pointer" }}>
                                Category {sortOrderCategory === "asc" ? "▲" : sortOrderCategory === "desc" ? "▼" : ""}
                                </th>
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
        </main>
    );
};