import React, { useRef, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Carousel, Spinner } from 'react-bootstrap';
import axios from 'axios';
import toast, { Toaster } from 'react-hot-toast';
import './MyItemsPage.css';


function MyItemsPage() {
    const [items, setItems] = useState(null);
    const navigate = useNavigate();


    useEffect(() => {
        const fetchUserLogin = async () => {
            try {
                const response = await axios.get('api/user/isloggedin/0');
                if (response.status === 200) {
                    console.log('User is logged in.');
                }
            } catch (error) {
                if (error.response.status === 401) {
                    toast.error('Jūs turite būti prisijungęs');
                    navigate('/prisijungimas');
                } else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            }
        };
        fetchUserLogin();
    }, []);

    useEffect(() => {
        async function fetchItems() {
            try {
                const response = await axios.get('/api/item/getUserItems');
                setItems(response.data);
            } catch (error) {
                console.error('Error fetching user items:', error);
            }
        }
        fetchItems();
    }, []);

    const handleOpen = (itemId) => {
        navigate(`/skelbimas/${itemId}`);
    }

    return items ? (
        <Container className="home">
            {items.length > 0 && (
                <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Jūsų skelbimai</h3>
            )}
            {items.length === 0 ? (
                <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Jūs neturite įkėlęs skelbimo</h3>
            ) : (
                <Row>
                    {items.map((item) => (
                        <Col sm={4} key={item.id}>
                            <Card className="mb-4">
                                <img
                                    className="d-block w-100"
                                    style={{ objectFit: "cover" }}
                                    height="256"
                                    src={item.images && item.images.length > 0 ? `data:image/png;base64,${item.images[0].data}` : ""}
                                    alt={item.name}
                                />
                                <Card.Body>
                                    <Card.Title>{item.name}</Card.Title>
                                    <Card.Text>{item.description}</Card.Text>
                                    <ul className="list-group list-group-flush mb-3">
                                        <li className="list-group-item d-flex justify-content-between align-items-center">
                                            <span>{item.type}</span>
                                            <span>{item.location}</span>
                                        </li>
                                        <li className="list-group-item d-flex justify-content-between align-items-center">
                                            <span>Baigiasi:</span>
                                            <span>{new Date(item.endDateTime).toLocaleString('lt-LT').slice(5, -3)}</span>
                                        </li>
                                    </ul>
                                    <div className="d-flex justify-content-end">
                                        <Button variant="primary" onClick={() => handleOpen(item.id)}>Peržiūrėti</Button>
                                    </div>
                                </Card.Body>
                            </Card>
                        </Col>
                    ))}
                </Row>
            )}
        </Container>
    ) : (
        <Container className="my-5">
            <div className='outerBoxWrapper d-flex justify-content-center'>
                <Spinner animation="border" role="status" />
            </div>
        </Container>
    );
}
export default MyItemsPage;
