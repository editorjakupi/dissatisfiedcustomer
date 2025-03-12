import React, { useEffect, useState } from "react";

const FeedbackView = ({ user }) => {
    const [tickets, setTickets] = useState([]);
    const [sortedTickets, setSortedTickets] = useState([]);
    const [sortConfig, setSortConfig] = useState({ key: "date", direction: "asc" });
    const [selectedTicket, setSelectedTicket] = useState(null);

    useEffect(() => {
        fetch(`/api/tickets/feedback`)
            .then((res) => res.json())
            .then((data) => {
                console.log("Ticket Feedback Response:", data);
                setTickets(data);
                setSortedTickets(data);
            })
            .catch((error) => console.error("Error fetching ticket feedback:", error));
    }, []);

    const sortData = (key) => {
        let direction = sortConfig.key === key && sortConfig.direction === "asc" ? "desc" : "asc";

        const sorted = [...tickets].sort((a, b) => {
            let valA = a[key];
            let valB = b[key];

            if (key === "date") {
                valA = new Date(a.date);
                valB = new Date(b.date);
            } else if (key === "rating") {
                valA = a.rating || 0;
                valB = b.rating || 0;
            } else {
                valA = valA?.toString().toLowerCase() || "";
                valB = valB?.toString().toLowerCase() || "";
            }

            if (valA < valB) return direction === "asc" ? -1 : 1;
            if (valA > valB) return direction === "asc" ? 1 : -1;
            return 0;
        });

        setSortedTickets(sorted);
        setSortConfig({ key, direction });
    };

    const handleTicketClick = (ticket) => {
        setSelectedTicket(ticket.id === selectedTicket?.id ? null : ticket);
    };

    return (
        <div>
            <h2>Ticket Feedback</h2>
            {sortedTickets.length > 0 ? (
                <div className="table">
                    {/* Table Header with Sorting */}
                    <div className="table-header">
                        <div className="table-cell sortable" onClick={() => sortData("title")}>
                            Title {sortConfig.key === "title" ? (sortConfig.direction === "asc" ? "▲" : "▼") : ""}
                        </div>
                        <div className="table-cell sortable" onClick={() => sortData("date")}>
                            Date {sortConfig.key === "date" ? (sortConfig.direction === "asc" ? "▲" : "▼") : ""}
                        </div>
                        <div className="table-cell sortable" onClick={() => sortData("userEmail")}>
                            Customer {sortConfig.key === "userEmail" ? (sortConfig.direction === "asc" ? "▲" : "▼") : ""}
                        </div>
                        <div className="table-cell sortable" onClick={() => sortData("employeeName")}>
                            Employee {sortConfig.key === "employeeName" ? (sortConfig.direction === "asc" ? "▲" : "▼") : ""}
                        </div>
                        <div className="table-cell sortable" onClick={() => sortData("rating")}>
                            Rating {sortConfig.key === "rating" ? (sortConfig.direction === "asc" ? "▲" : "▼") : ""}
                        </div>
                    </div>

                    {/* Table Rows */}
                    {sortedTickets.map((ticket) => (
                        <React.Fragment key={ticket.id}>
                            <div className="table-row" onClick={() => handleTicketClick(ticket)}>
                                <div className="table-cell">{ticket.title}</div>
                                <div className="table-cell">{new Date(ticket.date).toLocaleDateString()}</div>
                                <div className="table-cell">{ticket.userEmail || "N/A"}</div>
                                <div className="table-cell">{ticket.employeeName || "N/A"}</div>
                                <div className="table-cell">{ticket.rating ? `${ticket.rating}/5` : "No rating"}</div>
                            </div>
                            {selectedTicket?.id === ticket.id && (
                                <div className="comment-box">
                                    <p><strong>Comment:</strong> {ticket.comment || "No feedback yet."}</p>
                                </div>
                            )}
                        </React.Fragment>
                    ))}
                </div>
            ) : (
                <p>No tickets found for this company.</p>
            )}
        </div>
    );
};

export default FeedbackView;
