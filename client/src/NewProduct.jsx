import React, {useState} from "react";
import "./new.css";

const NewProduct = () => {
    const [formData, setFormData] = useState({
        name: "",
        description: "",
        companyId: "",
    });

    const [message, setMessage] = useState("");
    
    const handleChange = (e) => {
        setFormData({...formData, [e.target.name]: e.target.value});
    };

    const [products, setProducts] = useState([]);
    const [selectedProducts, setSelectedProduct] = useState(null);
    const [searchId, setSearchId] = useState("");

    const handleSearch = () => {
        if (!searchId.trim()) return;

        fetch(`/api/product/${searchId}`)
            .then((res) => {
                if (!res.ok) {
                    throw new Error(`Error: ${res.status} - ${res.statusText}`);
                }
                return res.json();
            })
            .then((data) => {
                console.log("Fetched product data:", data); // Log the fetched data
                if (data) {
                    setProducts(data); // Set products to the single product
                    setSelectedProduct(data); // Set selected product
                }
            })
            .catch((err) => {
                console.error("Search error:", err.message);
                setProducts([]); // Clear products list if error
                setSelectedProduct(null); // Clear selected product
            });
    };


    // Show all products again
    const handleShowAll = () => {
        fetch("/api/products/1")//Replace 1 with sessionID ( next sprint )
            .then((res) => res.json())
            .then((data) => {
                setProducts(data); // Restore full list of products
                setSelectedProduct(null); // Clear selected product
                setSearchId(""); // Reset search field
            })
            .catch((err) => console.error("Error fetching products:", err));
    };

    const handleDelete = () => {
        if (!selectedProducts) return;

        fetch(`/api/products/${selectedProducts.id}`, {
            method: 'DELETE',
        })
            .then((res) => {
                if (res.ok) {
                    setProducts((prevproducts) => prevproducts.filter((product) => product.id !== selectedProducts.id));
                    setSelectedProduct(null);
                } else {
                    console.error("Error deleting product.");
                }
            })
            .catch((err) => {
                console.error("Delete error:", err);
            });
    };

    console.log("products state:", products); // Log the products state
    console.log("Selected product state:", selectedProducts); // Log the selected product state

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage("");

        try {
            // Create the product
            const productResponse = await fetch("/api/products", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: formData.name,
                    description: formData.description,
                    companyId: formData.companyId,
                }),
            });

            // Debugging: Log full response
            const responseText = await productResponse.text();
            console.log("Product API full response:", responseText);
            console.log("Product API response status:", productResponse.status);

            if (!productResponse.ok) throw new Error(responseText || "Failed to create product");
            
            setMessage("Product created successfully!");
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
                        placeholder="Enter product ID"
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
                            {products.length > 0 ? (
                                products.map((product) => (
                                    <div
                                        key={product.id}
                                        className="user-item"
                                        onClick={() => setSelectedProduct(product)}
                                    >
                                        {product.name}
                                    </div>
                                ))
                            ) : (
                                <p>No products found.</p>
                            )}
                        </div>

                        {/* product Details */}
                        <div className="user-details">
                            {selectedProducts ? (
                                <div className="user-card">
                                    <h2>{selectedProducts.name}</h2>
                                    <p>
                                        <strong>Description:</strong> {selectedProducts.description}
                                    </p>
                                    <button onClick={handleDelete} className="delete-button">
                                        Delete product
                                    </button>
                                </div>
                            ) : (
                                <p className="user-placeholder">Select a product to see details</p>
                            )}
                        </div>
                    </div>
                    <div className="form-container">
                        <form onSubmit={handleSubmit} className="form">
                            <h2>Create New Product:</h2>
                            <input type="text" name="name" value={formData.name} placeholder="Name"
                                   onChange={handleChange}
                                   required/>
                            <input type="text" name="description" value={formData.description} placeholder="Description"
                                   onChange={handleChange} required/>
                            <input type="text" name="companyId" value={formData.companyId} placeholder="Comapny ID"
                                   onChange={handleChange} required/>
                            <button type="submit">Create Product</button>
                        </form>
                        {message && <p>{message}</p>}
                    </div>
                </div>
            </div>
        </main>
    )
};

export default NewProduct;