import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Carousel, Col, Container, Row, Form, Button, Card, Spinner } from 'react-bootstrap';
import toast, { Toaster } from 'react-hot-toast';
import './DeviceWinnerPage.css'
import axios from 'axios';

export const DeviceWinnerPage = () => {
    const { deviceId } = useParams();
    const [viewerId, setViewerId] = useState(null);
    const [canAccess, setCanAccess] = useState(null);
    const [phone, setPhone] = useState('');
    const [message, setMessage] = useState('');
    const [device, setDevice] = useState(null);
    const [posterEmail, setPosterEmail] = useState(null);
    const [sending, setSending] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchViewerId = async () => {
            try {
                const response = await axios.get('api/user/getCurrentUserId');
                setViewerId(response.data);

                if (response.data === device?.winnerId && (device?.status === 'Išrinktas laimėtojas' || device?.status === 'Laukiama patvirtinimo' || device?.status === 'Atiduotas')) {
                    setCanAccess(true);
                } else if (device && response.data !== device?.winnerId) {
                    navigate('/');
                }
            } catch (error) {
                if (error.response.status === 401) {
                    navigate('/prisijungimas');
                    toast.error('Turite būti prisijungęs!');
                }
                else {
                    navigate('/index');
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            }
        };
        fetchViewerId();
    }, [device]);

    useEffect(() => {
        const fetchDevice = async () => {
            try {
                const response = await axios.get(`api/device/getDevice/${deviceId}`);
                setDevice(response.data);
            } catch (error) {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };
        fetchDevice();
    }, [deviceId, canAccess]);

    useEffect(() => {
        const fetchPosterEmail = async () => {
            try {
                const response = await axios.get(`api/user/getUserEmail/${device.userId}`);
                setPosterEmail(response.data);
            } catch (error) {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };
        if (device && device.userId) {
            fetchPosterEmail();
        }
    }, [device]);


    const handleMessageChange = (event) => {
        setMessage(event.target.value);
    };

    const handlePhoneChange = (event) => {
        setPhone(event.target.value);
    };

    const handleSubmit = (event) => {
        event.preventDefault();

        if (phone.length < 9) {
            toast.error('Įveskite telefono numerį!');
            return;
        }

        const data = {
            phone: phone,
            message: message,
            deviceId: deviceId,
            deviceName: device.name,
            posterEmail: posterEmail,
            winnerId: viewerId
        }

        setSending(true);
        axios.post('api/devicewinner/submitWinnerDetails', data)
            .then(response => {
                if (response.data) {
                    toast.success('Sėkmingai išsiųstas pranešimas skelbėjui!');
                }
                else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                    setSending(false);
                }
            })
            .catch(error => {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                setSending(false);
            });
    };
    
    const handleConfirm = async () => {
        try {
            const data = {
                id: deviceId,
                winnerId: device.winnerId,
                userId: device.userId
            };
    
            const response = await axios.post('api/devicewinner/confirm', data);
            toast.success('Sėkmingai patvirtinta!');
        } catch (error) {
            toast.error('Įvyko klaida patvirtinant!');
        }
    };
    

    return device && viewerId && canAccess && posterEmail ? (
        <div className='outerBoxWrapper'>
            <Toaster />
            <Container className="my-5">
                <Row>
                    <Col md={4}>
                        {device.images && device.images.length > 0 && (
                            <Carousel>
                                {device.images.map((image, index) => (
                                    <Carousel.Item key={index}>
                                        <img className="d-block w-100" 
                                        src={`data:image/png;base64,${image.data}`}
                                        alt={`Image ${index + 1}`}
                                        height="320"
                                        style={{ border: '1px solid white' }}
                                            />
                                    </Carousel.Item>
                                ))}
                            </Carousel>
                        )}
                    </Col>
                    <Col md={8}>
                        <Card>
                            <Card.Header>{device.category}</Card.Header>
                            <Card.Body>
                                <Card.Title>Laimėjote: {device.name}</Card.Title>
                                <Card.Text>{device.description}</Card.Text>
                                <hr></hr>
                                {device.status === 'Išrinktas laimėtojas' && (
        <>
            <Card.Text>Norint suderinti atsiėmimą ar pristatymą, pateikite savo kontaktinius duomenis, su kuriais skelbėjas galės su Jumis susisiekti:</Card.Text>
            <Form onSubmit={handleSubmit}>
                <Form.Group>
                    <Form.Label>Telefono numeris:</Form.Label>
                    <Form.Control as="textarea" rows={1} onChange={handlePhoneChange} type="phone" placeholder="Telefono numeris" />
                    <br />
                    <Form.Label>Žinutė:</Form.Label>
                    <Form.Control as="textarea" rows={3} onChange={handleMessageChange} placeholder="Papildoma informacija (nebūtina)" />
                </Form.Group>
                <Button variant="primary" disabled={sending} type="submit">Siųsti</Button>
            </Form>
            <br />
        </>
    )}

{device.status === 'Laukiama patvirtinimo' && (
    <Button variant="primary" onClick={handleConfirm}>Patvirtinti</Button>
)}

                                <br></br>
                            </Card.Body>
                            <Card.Footer>{device.location} | Skelbėjo el. paštas: {posterEmail}</Card.Footer>
                        </Card>
                    </Col>
                </Row>
            </Container>
        </div>
    ) : (
        <Container className="my-5">
            <div className='outerBoxWrapper d-flex justify-content-center'>
                <Spinner animation="border" role="status" />
            </div>
        </Container>
    );    
}