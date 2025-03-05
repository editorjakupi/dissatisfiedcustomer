import React, {useState} from "react";
import "./new.css";

const NewEmployee = ({ user, setUser }) => {
    const [formData, setFormData] = useState({
        name: "",
        email: "",
        password: "",
        phonenumber: "",
        companyId: "",
    });
    
    const [message, setMessage] = useState("");
    
    const handleChange = (e) => {
        setFormData({...formData, [e.target.name]: e.target.value});
    };
    
    const [employees, setEmployees] = useState([]);
    const [selectedEmployees, setSelectedEmployee] = useState(null);
    const [searchId, setSearchId] = useState("");


    const handleSearch = () => {
        if (!searchId.trim()) return;

        fetch(`/api/employee/${searchId}`)
            .then((res) => {
                if (!res.ok) {
                    throw new Error(`Error: ${res.status} - ${res.statusText}`);
                }
                return res.json();
            })
            .then((data) => {
                console.log("Fetched Employees data:", data); // Log the fetched data
                if (data) {
                    setEmployees(data); // Set employees to the single product
                    setSelectedEmployee(data); // Set selected product
                }
            })
            .catch((err) => {
                console.error("Search error:", err.message);
                setEmployees([]); // Clear employees list if error
                setSelectedEmployee(null); // Clear selected product
            });
    };


    // Show all employees again
    const handleShowAll = () => {
        fetch(`/api/employee/${user.companyId}`)//Replace 1 with sessionID ( next sprint )
            .then((res) => res.json())
            .then((data) => {
                setEmployees(data); // Restore full list of employees
                setSelectedEmployee(null); // Clear selected product
                setSearchId(""); // Reset search field
            })
            .catch((err) => console.error("Error fetching employees:", err));
    };

    const handleDelete = () => {
        if (!selectedEmployees) return;

        fetch(`/api/employees/${selectedEmployees.id}`, {
            method: 'DELETE',
        })
            .then((res) => {
                if (res.ok) {
                    setEmployees((prevemployees) => prevemployees.filter((product) => product.id !== selectedEmployees.id));
                    setSelectedEmployee(null);
                } else {
                    console.error("Error deleting Employee.");
                }
            })
            .catch((err) => {
                console.error("Delete error:", err);
            });
    };

    console.log("Employees state:", employees); // Log the employees state
    console.log("Selected employees state:", selectedEmployees); // Log the selected product state


    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage("");

        try {
            // Create the user
            const userResponse = await fetch("/api/users", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: formData.name,
                    email: formData.email,
                    password: formData.password,
                    phonenumber: formData.phonenumber,
                }),
            });

            // Debugging: Log full response
            const responseText = await userResponse.text();
            console.log("User API full response:", responseText);
            console.log("User API response status:", userResponse.status);

            if (!userResponse.ok) throw new Error(responseText || "Failed to create user");

            // Try parsing response as JSON first
            let userId;
            try {
                userId = JSON.parse(responseText); // Expecting an integer as a response
            } catch (error) {
                console.error("Failed to parse response as JSON:", error);
            }

            // Fallback to parseInt if JSON parsing fails
            if (isNaN(userId)) {
                userId = parseInt(responseText.trim(), 10);
            }
            console.log("Created user ID:", userId);

            if (isNaN(userId)) throw new Error("Invalid user ID received");

            // Send employee request
            const employeeData = {
                userId,
                companyId: parseInt(user.companyId, 10), // Ensure companyId is a number
            };

            console.log("Sending employee data:", JSON.stringify(employeeData));

            const employeeResponse = await fetch("/api/employees", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(employeeData),
            });

            const employeeResponseText = await employeeResponse.text();
            console.log("Employee API response:", employeeResponseText);

            if (!employeeResponse.ok) throw new Error(employeeResponseText || "Failed to add employee");

            setMessage("User and Employee created successfully ID: " + userId);
        } catch (error) {
            console.error(error);
            setMessage(error.message);
        }
    };



    return (
        <main>
            <div className="user-container">
                {/* Search Bar */}
                <div className="search-container">
                    <input
                        type="text"
                        placeholder="Enter Employee ID"
                        value={searchId}
                        onChange={(e) => setSearchId(e.target.value)}
                        className="search-input"
                    />
                    <div className="button-container">
                        <button onClick={handleSearch} className="search-button">Search</button>
                        <button onClick={handleShowAll} className="show-all-button">Show All</button>
                    </div>
                </div>

                <div className="main-container">
                    {/* product List & Details */}
                    <div className="content-wrapper">
                        {/* product List */}
                        <div className="users-list">
                            {employees.length > 0 ? (
                                employees.map((employee) => (
                                    <div
                                        key={employee.id}
                                        className="user-item"
                                        onClick={() => setSelectedEmployee(employee)}
                                    >
                                        {employee.name}
                                    </div>
                                ))
                            ) : (
                                <p>No Employees found.</p>
                            )}
                        </div>

                        {/* product Details */}
                        <div className="user-details">
                            {selectedEmployees ? (
                                <div className="user-card">
                                    <h2>{selectedEmployees.name}</h2>
                                    <p>
                                        <strong>Name:</strong> {selectedEmployees.name}
                                    </p>
                                    <p>
                                        <strong>Phonenumber:</strong> {selectedEmployees.phonenumber}
                                    </p>
                                    <p>
                                        <strong>Password:</strong> {selectedEmployees.password}
                                    </p>
                                    <button onClick={handleDelete} className="delete-button">
                                        Delete Employee
                                    </button>
                                </div>
                            ) : (
                                <p className="user-placeholder">Select a Employee to see details</p>
                            )}
                        </div>
                    </div>
                    <div className="form-container">
                        <form onSubmit={handleSubmit} className="form">
                            <h2>Create New Employee:</h2>
                            <input type="text" name="name" value={formData.name} placeholder="Name"
                                   onChange={handleChange} required/>
                            <input type="email" name="email" value={formData.email} placeholder="Email"
                                   onChange={handleChange} required/>
                            <input type="text" name="phonenumber" value={formData.phonenumber} placeholder="Phone Number"
                                   onChange={handleChange} required/>
                            <button type="submit">Create Employee</button>
                        </form>
                        {message && <p>{message}</p>}
                    </div>
                </div>
            </div>
        </main>
    )
};

export default NewEmployee;