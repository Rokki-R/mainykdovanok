import React, { useState, useEffect } from "react";
import { useParams } from 'react-router-dom';
import { useNavigate } from 'react-router';
import { Container, Row, Col, Card, ListGroup, ListGroupItem, Button, Spinner, Carousel } from "react-bootstrap";
import toast, { Toaster } from 'react-hot-toast';
import axios from 'axios';
import './DetailedItemInfoPage.css';

export const DetailedItemInfoPage = () => {
    const { itemId } = useParams();
    const [item, setItem] = useState(null);
    const [isLoggedInAsAdmin, setIsLoggedInAsAdmin] = useState(false);
    const [viewerId, setViewerId] = useState(null);
    const [itemQuestions_Answers, setItemQuestions_Answers] = useState(null);
    const [itemOffers, setItemOffers] = useState(null);
    const [itemLotteryParticipants, setItemLotteryParticipants] = useState(null);
    const [isSubmitting, setSubmitting] = useState(false);
    const navigate = useNavigate();

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
    }, [itemId]);

    useEffect(() => {
        const fetchUserRole = async () => {
            try {
                const response = await axios.get('api/user/isloggedin/1');
                if (response.status == 200) {
                    setIsLoggedInAsAdmin(true);
                }
            } catch (error) {
                if (error.response.status === 401) {
                    setIsLoggedInAsAdmin(false);
                }
                else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            }
        };
        fetchUserRole();
    }, []);

    useEffect(() => {
        const fetchViewerId = async () => {
            try {
                const response = await axios.get('api/user/getCurrentUserId');
                setViewerId(response.data);
            } catch (error) {
                if (error.response.status === 401) {
                    navigate('/prisijungimas');
                    toast.error('Turite būti prisijungęs!');
                }
                else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            }
        };
        fetchViewerId();
    }, []);

    const handleSubmit = (event) => {
        event.preventDefault();

    };

    if (!isLoggedInAsAdmin) {
        if (item && viewerId && item.userId !== viewerId) {
            alert('Jūs nesate šio skelbimo savininkas!');
            navigate(`/`);
        }
    }

    //Palieku kaip atskirus, bet gal bekuriant endpointus iseis kazkaip apjungt ar kaip tik reiks ju daugiau?
    useEffect(() => {
        const fetchItemQuestions_Answers = async () => {
            try {
                const response = await axios.get(`api/item/getQuestionsAndAnswers/${itemId}`);
                setItemQuestions_Answers(response.data);
            } catch (error) {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };

        fetchItemQuestions_Answers();
    }, []);
    
     useEffect(() => {
         const fetchItemOffers = async () => {
             try { 
                 const response = await axios.get(`api/item/getOffers/${itemId}`);
                 setItemOffers(response.data);
             } catch (error) {
                 toast.error('Įvyko klaida, susisiekite su administratoriumi!');
             }
         };
    
         fetchItemOffers();
     }, []);
        
    
        useEffect(() => {
            const fetchItemLotteryParticipants = async () => {
                try {
                    const response = await axios.get(`api/item/getLotteryParticipants/${itemId}`);
                    setItemLotteryParticipants(response.data);
                } catch (error) {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            };
    
            fetchItemLotteryParticipants();
        }, []);

        const handleChosenWinner = async (user) => {
            const requestBody = {
                itemId,
                user
            };
            setSubmitting(true);
            
            await axios.post(`/api/item/chooseQuestionnaireWinner`, requestBody)
            .then(response => {
                if (response) {
                    toast.success('Išsirinkote, kam padovanoti! Laimėtojui išsiųstas el. laiškas dėl susisiekimo.');
                    navigate(`/`);
                }
                else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            })
            .catch(error => {
                if (error.response.status === 401) {
                    toast.success('Turite būti prisijungęs!');
                }
                else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');   
                }
            });
            setSubmitting(false);
    };

    const handleOfferWinner = async (user, itemName, userItemId) => {
        const requestBody = {
            itemId,
            user,
            itemName,
            userItemId
        };
        setSubmitting(true);

        await axios.post(`/api/item/chooseExchangeOfferWinner`, requestBody)
            .then(response => {
                if (response) {
                    toast.success('Išsirinkote, su kuo mainyti! Laimėtojui išsiųstas el. laiškas dėl susisiekimo.');
                    navigate(`/`);
                }
                else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            })
            .catch(error => {
                if (error.response.status === 401) {
                    toast.error('Turite būti prisijungęs!');
                }
                else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            });
        setSubmitting(false);
    };

    return item && ((item.type === 'Klausimynas' && itemQuestions_Answers) || (item.type === 'Loterija' && itemLotteryParticipants) || (item.type === 'Mainai į kita prietaisą' && itemOffers)) ? (
        <div className="my-div" style={{ marginTop: "120px" }}>
            {item.type === 'Mainai į kita prietaisą' && (
                <Container className="home">
                    <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Pasiūlymai mainams</h3>
                    <Row className="justify-content-center">
                        {itemOffers.map((item) => (
                            <Col sm={4} key={item.id} style={{ width: '300px' }}>
                                <Card className="mb-4">
                                    <Carousel style={{ height: "250px" }} >
                                        {item.images && item.images.map((image, index) => (
                                            <Carousel.Item key={index}>
                                                <img
                                                    className="d-block w-100"
                                                    style={{ objectFit: "cover" }}
                                                    src={`data:image/png;base64,${image.data}`}
                                                    alt={item.name}
                                                />
                                            </Carousel.Item>
                                        ))}
                                    </Carousel>
                                    <Card.Body>
                                        <Card.Title>{item.name}</Card.Title>
                                        <Card.Text>{item.description}</Card.Text>
                                        <Card.Text>{item.message}</Card.Text>
                                        <Card.Text>-{item.user}</Card.Text>
                                        <ul className="list-group list-group-flush mb-3">
                                            <li className="list-group-item d-flex justify-content-between align-items-center">
                                                <span>{item.location}</span>
                                            </li>
                                            <li className="list-group-item d-flex justify-content-between align-items-center">
                                                <span>Baigiasi:</span>
                                                <span>{new Date(item.endDateTime).toLocaleString('lt-LT').slice(5, -3)}</span>
                                            </li>
                                        </ul>
                                        <div className="d-flex justify-content-end">
                                            <Button variant="primary" disabled={isSubmitting} onClick={() => handleOfferWinner(item.user, item.name, item.id)}>Mainyti!</Button>
                                        </div>
                                    </Card.Body>
                                </Card>
                            </Col>
                        ))}
                    </Row>
                </Container>
            )}

            {item.type === 'Klausimynas' && (
                <ListGroup>
                    {Object.keys(itemQuestions_Answers.questionnaires).length > 0 ? (
                        Object.keys(itemQuestions_Answers.questionnaires).map((user) => (
                            <Container key={user}>
                                <Button type="submit" variant="primary" disabled={isSubmitting} onClick={() => handleChosenWinner(user)}>Atiduoti</Button>
                                <ListGroupItem variant="primary"><b>Klausimyno atsakymai:</b> {user} </ListGroupItem>
                                {itemQuestions_Answers.questionnaires[user].map((questionnaire, index) => (
                                    <ListGroup key={questionnaire.id}>
                                        <ListGroupItem variant="info"><b>Klausimas nr. {index + 1}</b> {questionnaire.question} </ListGroupItem>
                                        <ListGroupItem variant="light"><b>Atsakymas:</b> {questionnaire.answer} </ListGroupItem>
                                    </ListGroup>
                                ))}
                            </Container>
                        ))
                    ) : (
                        <Container>
                            <ListGroupItem variant="primary"><b>Klausimyno atsakymai</b></ListGroupItem>
                            <ListGroupItem>Klausimyno atsakymų nėra.</ListGroupItem>
                        </Container>
                    )}
                </ListGroup>
            )}
            {item.type === 'Loterija' && (
                <ListGroup>
                    <ListGroupItem variant="primary"><b>Loterijos dalyviai</b></ListGroupItem>
                    {itemLotteryParticipants.length > 0 ? (
                        itemLotteryParticipants.map((user) => (
                            <ListGroupItem key={user.id} className="d-flex justify-content-between align-items-center">
                                <div className="d-flex align-items-center">
                                    {user.name} {user.surname}
                                </div>
                                <div className="d-flex align-items-center">
                                    <Button className='ReviewProfileButton' variant="primary" size="sm" onClick={() => navigate(`/klientas/${user.id}`)}>View Profile</Button>
                                </div>
                            </ListGroupItem>
                        ))
                    ) : (
                        <ListGroupItem>Loterijos dalyvių nėra.</ListGroupItem>
                    )}
                </ListGroup>
            )}
        </div>
    ) : (
        <Container className="my-5">
            <div className='outerBoxWrapper d-flex justify-content-center'>
                <Spinner animation="border" role="status" />
            </div>
        </Container>
    );
}