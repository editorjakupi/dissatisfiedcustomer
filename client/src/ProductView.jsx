import React, {useEffect, useState} from "react";
import { data } from "react-router";

const ProductView = () => {
    const [products, setProducts] = useState([]);

    useEffect(() => {
        fetch("/api/products/11") //hardcoded to see response
        .then((response) => response.json())
        .then((data) => {
            setProducts(data);
        
        })
    })

    return (
        <div className="product_container">
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    {products.map((product) => (
                        <tr key={product.id}>
                            <td>{product.name}</td>
                            <td>{product.description}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}

export default ProductView;