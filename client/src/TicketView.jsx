import React, { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import "./TicketView.css";

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

export default function TicketView() {
    const [tickets, setTickets] = useState([]);
    const [searchparams] = useSearchParams();
    const [sortedTickets, setSortedTickets] = useState([]);
    const [defaultOrder, setDefaultOrder] = useState([]);

    const [sortOrderTitle, setSortOrderTitle] = useState("default");
    const [sortOrderCategory, setSortOrderCategory] = useState("default");

    const view = searchparams.get("view");

    function MarkAsResolved(ticket) {
        // Save the original status in case of a rollback
        const originalStatus = ticket.status;

        // Optimistically update the ticket status
        setTickets(prevTickets =>
            prevTickets.map(t => t.id === ticket.id ? { ...t, status: "Resolved" } : t)
        );

        // Make the API call to update the status on the server
        fetch(`/api/tickets/${ticket.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ status_id: 3 }),
        })
            .then((response) => {
                if (!response.ok) {
                    // If the request fails, revert the status back to original
                    setTickets(prevTickets =>
                        prevTickets.map(t => t.id === ticket.id ? { ...t, status: originalStatus } : t)
                    );
                    console.error("Error marking ticket as resolved.");
                }
            })
            .catch((error) => {
                // If the API call fails (network issues, etc.), revert the status back to original
                setTickets(prevTickets =>
                    prevTickets.map(t => t.id === ticket.id ? { ...t, status: originalStatus } : t)
                );
                console.error("Error marking ticket as resolved:", error);
            });
    }


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

    function TableItem({ id, date, title, categoryname, email, status }) {
        return (
            <tr key={id}>
                <td>{date}</td>
                <td>{title}</td>
                <td>{categoryname}</td>
                <td>{email}</td>
                <td>{status}</td>
                <td>
                    {status !== "Resolved" ? (
                        <button className="resolve-button" onClick={() => MarkAsResolved({ id, date, title, categoryname, email, status })}>
                            Mark as Resolved
                        </button>
                    ) : (
                        <span>Resolved</span>
                    )}
                </td>
            </tr>
        );
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
        </>
    );
};
