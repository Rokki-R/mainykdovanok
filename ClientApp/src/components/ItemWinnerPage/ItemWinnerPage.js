import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Carousel, Col, Container, Row, Form, Button, Card, Spinner } from 'react-bootstrap';
import toast, { Toaster } from 'react-hot-toast';
import './ItemWinnerPage.css'
import axios from 'axios';

export const ItemWinnerPage = () => {
    const { itemId } = useParams();
    const [viewerId, setViewerId] = useState(null);
    const [canAccess, setCanAccess] = useState(null);
    const [phone, setPhone] = useState('');
    const [message, setMessage] = useState('');
    const [item, setItem] = useState(null);
    const [posterEmail, setPosterEmail] = useState(null);
    const [sending, setSending] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchViewerId = async () => {
            try {
                const response = await axios.get('api/user/getCurrentUserId');
                setViewerId(response.data);

                if (response.data === item?.winnerId && item?.status === 'Išrinktas laimėtojas') {
                    setCanAccess(true);
                } else if (item && response.data !== item?.winnerId) {
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
    }, [item]);

    useEffect(() => {
        const fetchItem = async () => {
            try {
                const response = await axios.get(`api/item/getItem/${itemId}`);
                setItem(response.data);
            } catch (error) {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };
        fetchItem();
    }, [itemId, canAccess]);

    useEffect(() => {
        const fetchPosterEmail = async () => {
            try {
                const response = await axios.get(`api/user/getUserEmail/${item.userId}`);
                setPosterEmail(response.data);
            } catch (error) {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };
        if (item && item.userId) {
            fetchPosterEmail();
        }
    }, [item]);


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
            itemId: itemId,
            itemName: item.name,
            posterEmail: posterEmail,
            winnerId: viewerId
        }

        setSending(true);
        axios.post('api/item/submitWinnerDetails', data)
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

    return item && viewerId && canAccess && posterEmail ? (
        <div className='outerBoxWrapper'>
            <Toaster />
            <Container className="my-5">
                <Row>
                    <Col md={4}>
                        {item.images && item.images.length > 0 && (
                            <Carousel>
                                {item.images.map((image, index) => (
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
                            <Card.Header>{item.category}</Card.Header>
                            <Card.Body>
                                <Card.Title>Laimėjote: {item.name}</Card.Title>
                                <Card.Text>{item.description}</Card.Text>
                                <hr></hr>
                                <Card.Text>Norint suderinti atsiėmimą ar pristatymą, pateikite savo kontaktinius duomenis, su kuriais skelbėjas galės su Jumis susisiekti:</Card.Text>
                                <Form onSubmit={handleSubmit}>
                                    <Form.Group>
                                        <Form.Label>Telefono numeris:</Form.Label>
                                        <Form.Control as="textarea" rows={1} onChange={handlePhoneChange} type="phone" placeholder="Telefono numeris" />
                                        <br></br>
                                        <Form.Label>Žinutė:</Form.Label>
                                        <Form.Control as="textarea" rows={3} onChange={handleMessageChange} placeholder="Papildoma informacija (nebūtina)" />
                                    </Form.Group>
                                    <Button variant="primary" disabled={sending} type="submit">Siųsti</Button>
                                </Form>
                                <br></br>
                            </Card.Body>
                            <Card.Footer>{item.location} | Skelbėjo el. paštas: {posterEmail}</Card.Footer>
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