import React, {useState} from "react";

const NewCompany = ({user, setUser}) => {
    const [formData, setFormData] = useState({
        name: "",
        phone: "",
        email: "",
    });

    const handleChange = (event) => 
        setFormData({...formData, [event.target.name]: event.target.value}); 

    const [companies, setCompanies] = useState([]);
    const [selectedCompany, setSelectedCompany] = useState(null);
    const [searchId, setSearchId] = useState("");

    //Company search
    const handleSearch = () => {
        //if (!searchId.trim()) return;

        fetch('/api/company/' + searchId)
        .then((response) => {
            /*if(!response.ok)
                throw new Error("Error: " + response.status + "-" + response.statusText);*/
            return response.json();
        })
        .then((data) => {
            if(data){
                setCompanies(data);
                setSelectedCompany(data);
            }

        })
        .catch((error) => {
            console.error("company search failed: ", error);
            setCompanies([]);
            setSelectedCompany(null);
            setSearchId("");
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

    //Company delete
    const handleDelete = () => {
        //if(!selectedCompany) return;

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

    //Company create
    const handleCreate = async(event) => {
        event.preventDeafult();
        setMessage("");

        try{
            const companyResponse = await fetch('/api/company', {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: formData.name,
                    phone: formData.phone,
                    email: formData.email,
                }),
            });
            const responseText = await companyResponse.text();
            if (!companyResponse.ok) throw new Error(responseText || "Failed to create company")
            alert("Company " + formData.name + " created sucessfully");
        }
        catch (error) {
            console.error(error);
            setMessage(error.message);
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
                                        onClick={() => setSelectedCompany(company)}
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
                                    <button onClick={handleDelete} className="delete-button">
                                        Delete company
                                    </button>
                                </div>
                            ) : (
                                <p className="user-placeholder">Select a company to see details</p>
                            )}
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
                    <div className="form-container">
                        <form onSubmit={handleCreate} className="form">
                            <h2>Create New company:</h2>
                            <input type="text" name="name" value={formData.name} placeholder="Name"
                                   onChange={handleChange}
                                   required/>
                            <input type="text" name="phone" value={formData.phone} placeholder="Phone"
                                   onChange={handleChange} required/>
                            <input type="test" name="email" value={formData.email} placeholder="Email"
                                    onClick={handleChange} required/>
                            <button type="submit">Create company</button>
                        </form>
                        {message && <p>{message}</p>}
                    </div>
*/
