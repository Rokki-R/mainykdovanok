import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Spinner } from 'react-bootstrap';
import axios from 'axios';
import './HomePage.css';

function HomePage() {
    const [devices, setDevices] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        async function fetchDevices() {
            try {
                const response = await axios.get('/api/home/getDevices');
                setDevices(response.data);
            } catch (error) {
                console.error('Error fetching devices:', error);
            }
        }
        fetchDevices();
    }, []);

    const handleOpen = (deviceId) => {
        navigate(`/skelbimas/${deviceId}`);
    }

    return (
        <Container className="home">
            <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Elektronikos prietaisų skelbimai</h3>
            <Row className="justify-content-center">
                {devices ? devices.map((device) => (
                    <Col sm={4} key={device.id} style={{ width: '300px' }}>
                        <Card className="mb-4">
                            <img
                                className="d-block w-100"
                                style={{ objectFit: "cover" }}
                                height="256"
                                src={device.images && device.images.length > 0 ? `data:image/png;base64,${device.images[0].data}` : ""}
                                alt={device.name}
                            />
                            <Card.Body>
                                <Card.Title>{device.name}</Card.Title>
                                <Card.Text>{device.description}</Card.Text>
                                <ul className="list-group list-group-flush mb-3">
                                    <li className="list-group-item d-flex justify-content-between align-items-center">
                                        <span>{device.type}</span>
                                        <span>{device.location}</span>
                                    </li>
                                </ul>
                                <div className="d-flex justify-content-end">
                                    <Button variant="primary" onClick={() => handleOpen(device.id)}>Peržiūrėti</Button>
                                </div>
                            </Card.Body>
                        </Card>
                    </Col>
                )) : (
                    <Col sm={12}>
                        <div className='outerBoxWrapper d-flex justify-content-center'>
                            <Spinner animation="border" role="status" />
                        </div>
                    </Col>
                )}
            </Row>
        </Container>
    );
}

export default HomePage;
