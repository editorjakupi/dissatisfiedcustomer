import React, { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
import "../../main.css";

function BoxesContainer() {
    // State to keep track of the different tickets
    const [ticketCounts, setTicketCounts] = useState({ active: 0, inactive: 0, resolved: 0, total: 0 });

    // Fetch ticket counts using view=all
    useEffect(() => {
        fetch("/api/tickets?view=all")
            .then((response) => response.json())
            .then((data) => {
                // Count the number of tickets with different statuses
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

    // Box-container with ticket counts
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
    const navigate = useNavigate();
    const [sortedTickets, setSortedTickets] = useState([]);
    const [defaultOrder, setDefaultOrder] = useState([]);

    // Using 2 different states to keep track of the sorting order for title and category
    const [sortOrderTitle, setSortOrderTitle] = useState("default");
    const [sortOrderCategory, setSortOrderCategory] = useState("default");
    const [companyId, setCompanyId] = useState(null);

    // Get the view parameter from the URL
    const view = searchparams.get("view");

    useEffect(() => {
        // Fetch the session to get the companyId
        fetch("/api/session")
            .then((response) => response.json())
            .then((data) => {
                const companyId = data?.companyId;
                if (companyId) {
                    setCompanyId(companyId);
                    console.log("Company ID:", companyId);
    
                    // Now make the ticket request with the correct companyId
                    fetch(`/api/tickets?view=${view}&companyId=${companyId}`)
                        .then((response) => response.json())
                        .then((data) => {
                            setTickets(data);
                            setSortedTickets(data);
                            setDefaultOrder(data);
                        })
                        .catch((error) => console.error("Error fetching tickets:", error));
                } else {
                    console.error("No companyId found in the session.");
                }
            })
            .catch((error) => console.error("Error fetching session:", error));
    }, [view]);  // This ensures the fetch happens again when `view` changes
    

    // Function to mark a ticket as resolved by pressing a button
    function MarkAsResolved(ticket) {
        const originalStatus = ticket.status;
    
        // Update the status of the ticket in the UI
        setTickets(prevTickets =>
            prevTickets.map(t => t.id === ticket.id ? { ...t, status: "Resolved" } : t)
        );
    
        setSortedTickets(prevSorted =>
            prevSorted.map(t => t.id === ticket.id ? { ...t, status: "Resolved" } : t)
        );
    
        fetch(`/api/tickets/${ticket.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ status_id: 3 }),
        })
            .then((response) => {
                if (!response.ok) {
                    throw new Error("Failed to update ticket status.");
                }
            })
            // If the request fails, revert the status of the ticket in the UI
            .catch((error) => {
                setTickets(prevTickets =>
                    prevTickets.map(t => t.id === ticket.id ? { ...t, status: originalStatus } : t)
                );
                setSortedTickets(prevSorted =>
                    prevSorted.map(t => t.id === ticket.id ? { ...t, status: originalStatus } : t)
                );
    
                console.error("Error marking ticket as resolved:", error);
            });
    }


    // Function to sort the tickets by title
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

        // Reset the category sorting order
        setSortOrderCategory("default");
        setSortedTickets(sorted);
    }

    // Function to sort the tickets by category
    function SortByCategory() {
        let sorted;

        if (sortOrderCategory === "default") {
            sorted = [...tickets].sort((a, b) => a.categoryName.localeCompare(b.categoryName, "sv"));
            setSortOrderCategory("asc");
        } else if (sortOrderCategory === "asc") {
            sorted = [...tickets].sort((a, b) => b.categoryName.localeCompare(a.categoryName, "sv"));
            setSortOrderCategory("desc");
        } else {
            sorted = [...defaultOrder];
            setSortOrderCategory("default");
        }

        // Reset the title sorting order
        setSortOrderTitle("default");
        setSortedTickets(sorted);
    }

    // Table Item component to display each ticket
    function TableItem({ id, date, title, categoryName, email, status }) {
        return (
            <tr key={id}>
                <td>{date}</td>
                <td className="ticketname" onClick={ () => navigate(`/tickets/handle/${id}`)}>{title}</td>
                <td>{categoryName}</td>
                <td>{email}</td>
                <td>{status}</td>
                <td>
                    {status !== "Resolved" ? (
                        <button className="resolve-button" onClick={() => MarkAsResolved({ id, date, title, categoryName, email, status })}>
                            Mark as Resolved
                        </button>
                    ) : (
                        <span>Resolved</span>
                    )}
                </td>
            </tr>
        );
    }

    // Rendering
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