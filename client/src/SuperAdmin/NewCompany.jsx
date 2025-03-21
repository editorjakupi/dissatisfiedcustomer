import React, {useState} from "react";
import "../main.css";

const NewCompany = () => {
    const [formData, setFormData] = useState({
        id: "",
        name: "",
        phone: "",
        email: "",
        admin: "",
    });
    const [adminData, setAdminData] = useState({
        id: "",
        name: "",
    });
    
    const [message, setMessage] = useState("");

    const handleChange = (event) => {
        const { name, value } = event.target;

        setFormData({
            ...formData,
            [name]: value === "" ? null : (name === "admin" || name === "id" ? parseInt(value) || null : value)
        });
    };

    const [companies, setCompanies] = useState([]);
    const [selectedCompany, setSelectedCompany] = useState(null);
    const [searchId, setSearchId] = useState("");
    const [admins, setAdmins] = useState([]);


    //Company search
    const handleSearch = () => {
        if (!searchId.trim()) return;

        fetch('/api/company/' + searchId)
        .then((response) => {
            if(!response.ok){
                throw new Error("Error: " + response.status + "-" + response.statusText);
            }
            return response.json();
        })
        .then((data) => {
            if(data){
                setCompanies(data);
                setSelectedCompany(data);
            }

        })
        .catch((error) => {
            console.error("company search failed: ", error.message);
            setCompanies([]);
            setSelectedCompany(null);
        });

    };

    //Company list
    const handleShowAll = () => {
        fetch('/api/company/')
        .then((response) => response.json())
        .then((data) => {
            setCompanies(data);
            setSelectedCompany(null);
            setSearchId("");
        })
        .catch((error) => console.error("company fetch failed: ", error));
    };

    //Admin list in edit/create form
    const handleShowAdmins = () => {
        fetch('/api/company/admins')
        .then((response) => response.json())
        .then((data) => {
            setAdmins(data);
            setAdminData(data);
        })
        .catch((error) => console.error("admin fetch failed: ",error));
    }

    //Company delete
    const handleDelete = () => {
        if(!selectedCompany) return;

        fetch('/api/company/' + selectedCompany.id, {
            method: 'DELETE',
        })
        .then((response) => {
            if (!response.ok){
                setCompanies((prev) => prev.filter((company) => company.id !== selectedCompany.id));
                setSelectedCompany(null);
                alert("Company " + selectedCompany.id + " was deleted");
            }else{
                console.error("error deleteing company");
            }
        })
        .catch((error) => {
            console.error("company delete failed:", error);
        });
    };
    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage("");

        if (selectedCompany) {  // Se till att selectedCompany inte är null
            try {
                const response = await fetch("/api/company/", {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        id: formData.id,
                        name: formData.name,
                        phone: formData.phone,
                        email: formData.email,
                        admin: formData.admin  
                    }),
                });
                console.log(selectedCompany.id);
                console.log("Sending data:", JSON.stringify(formData, null, 2));

                const responseText = await response.text();
                console.log("Company API full response:", responseText);
                console.log("Company API response status:", response.status);

                if (!response.ok) throw new Error(responseText || "Failed to update company");

                setMessage("Company updated successfully");
                // setCompanies((prev) =>
                //    prev.map(emp => emp.id === selectedCompany.id ? { ...emp, ...formData } : emp)
                // );
            } catch (error) {
                setMessage(error.message);
            }
        } else {  // Om selectedCompany är null, skapa en ny
            try {
                const companyResponse = await fetch("/api/company", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        name: formData.name,
                        phone: formData.phone,
                        email: formData.email,
                        admin: formData.admin
                    }),
                });

                console.log("Sending data:", JSON.stringify(formData, null, 2));

                const responseText = await companyResponse.text();
                console.log("Company API full response:", responseText);
                console.log("Company API response status:", companyResponse.status);

                if (!companyResponse.ok) throw new Error(responseText || "Failed to create company");

                setMessage("Company created successfully!");
            } catch (error) {
                console.error(error);
                setMessage("Please chose an admin");
            }
        }
    };


    return(
        <main>
            <div className="user-container">
                {/* Search Bar */}
                <div className="search-container">
                    <input
                        type="text"
                        placeholder="Enter company ID"
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
                    {/* company List & Details */}
                    <div className="content-wrapper">
                        {/* company List */}
                        <div className="users-list">
                            {companies.length > 0 ? (
                                companies.map((company) => (
                                    <div
                                        key={company.id}
                                        className="user-item"
                                        onClick={() => {
                                            setSelectedCompany(company)
                                            setFormData(company)
                                        }}
                                    >
                                        {company.name}
                                    </div>
                                ))
                            ) : (
                                <p>No companies found.</p>
                            )}
                        </div>

                        {/* company Details */}
                        <div className="user-details">
                            {selectedCompany ? (
                                <div className="user-card">
                                    <h2>{selectedCompany.name}</h2>
                                    <p>
                                        <strong>Name:</strong> {selectedCompany.name}
                                    </p>
                                    <p>
                                        <strong>Phone:</strong> {selectedCompany.phone}
                                    </p>
                                    <p>
                                        <strong>Email</strong> {selectedCompany.email}
                                    </p>
                                    <p>
                                        <strong>Admin</strong> {selectedCompany.admin}
                                    </p>
                                    <button onClick={handleDelete} className="delete-button">
                                        Delete company
                                    </button>
                                </div>
                            ) : (
                                <p className="user-placeholder">Select a company to see details</p>
                            )}
                        </div>
                        <div className="form-container">
                            <form onSubmit={handleSubmit} className="form">
                                <h2>{selectedCompany ? "Update Company" : "Create New Company"}</h2>
                                <input type="text" name="name" value={formData.name} placeholder="Name"
                                       onChange={handleChange} required/>
                                <input type="text" name="phone" value={formData.phone} placeholder="Phone"
                                       onChange={handleChange} required/>
                                <input type="text" name="email" value={formData.email} placeholder="Email"
                                       onChange={handleChange} required/>
                                <label>
                                Admin
                                    <select name="admin" id="admin-select" onClick={handleShowAdmins}
                                            onChange={handleChange}>
                                        <option value="">Select an admin</option>
                                        {admins.map((admin) => (
                                            <option key={admin.id} name="admin" value={admin.id}>{admin.name}</option>
                                        ))}
                                    </select>

                                </label>
                                <button type="submit">{selectedCompany ? "Update Company" : "Create Company"}</button>
                            </form>
                            {message && <p>{message}</p>}
                        </div>
                    </div>
                </div>
            </div>
        </main>
    )
};

export default NewCompany;



//update company
/*
                                {admins.length > 0 ? (
                                    admins.map((admin) => (
                                    <option value={admin.name}>{admin.name}</option>
                                    ))
                                ) : (
                                    <p>No admins found</p>
                                )}


                                <option value="admin1">Admin 1</option>
                                <option value="admin2">Admin 2</option>
*/
