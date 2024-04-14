import React, { useState, useEffect } from "react";
import { useParams } from 'react-router-dom';
import { useNavigate } from 'react-router';
import { Container, Row, Col, Card, ListGroup, ListGroupItem, Button, Spinner, Carousel } from "react-bootstrap";
import toast, { Toaster } from 'react-hot-toast';
import axios from 'axios';
import './DetailedDeviceInfoPage.css';

export const DetailedDeviceInfoPage = () => {
    const { deviceId } = useParams();
    const [device, setDevice] = useState(null);
    const [viewerId, setViewerId] = useState(null);
    const [deviceLetters, setDeviceLetters] = useState(null);
    const [deviceOffers, setDeviceOffers] = useState(null);
    const [deviceLotteryParticipants, setDeviceLotteryParticipants] = useState(null);
    const [isSubmitting, setSubmitting] = useState(false);
    const [deviceQuestions_Answers, setDeviceQuestions_Answers] = useState(null);
    const navigate = useNavigate();

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
    }, [deviceId]);

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
    
    useEffect(() => {
        if (device && viewerId && device.userId !== viewerId) {
            navigate('/');
            toast.error('Negalite peržiūrėti šio skelbimo informacijos');
        }
    }, [device, viewerId, navigate]);

    const handleSubmit = (event) => {
        event.preventDefault();

    };

    useEffect(() => {
        const fetchData = async () => {
            try {
                if (device && device.type === 'Motyvacinis laiškas') {
                    const response = await axios.get(`api/device/getLetters/${deviceId}`);
                    setDeviceLetters(response.data);
                } else if (device && device.type === 'Mainai į kita prietaisą') {
                    const response = await axios.get(`api/device/getOffers/${deviceId}`);
                    setDeviceOffers(response.data);
                } else if (device && device.type === 'Loterija') {
                    const response = await axios.get(`api/device/getLotteryParticipants/${deviceId}`);
                    setDeviceLotteryParticipants(response.data);
                } else if (device && device.type === 'Klausimynas') {
                    const response = await axios.get(`api/device/getQuestionsAndAnswers/${deviceId}`);
                    setDeviceQuestions_Answers(response.data);
                }
            } catch (error) {
                if(error.status.code === 403)
                {
                    toast.error("Jūs neturite prieigos peržiūrėti šio skelbimo informaciją")
                }
                else {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            }
        };
    
        fetchData();
    }, [device, deviceId]);
    
    const handleChosenWinner = async (deviceType, user) => {
        const requestBody = {
            deviceType: deviceType,
            user: user, 
            deviceId: deviceId
        };
        console.log(deviceType, user, deviceId)
        setSubmitting(true);
        
        await axios.post(`/api/device/chooseWinner`, requestBody)
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
            else if (error.response.status === 403) {
                toast.error('Jūs negalite atiduoti ne savo skelbimą!')
            }
            else {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');   
            }
        });
        setSubmitting(false);
};

    const handleOfferWinner = async (user, deviceName, userDeviceId) => {
        const requestBody = {
            deviceId,
            user,
            deviceName,
            userDeviceId
        };
        setSubmitting(true);

        await axios.post(`/api/device/chooseExchangeOfferWinner`, requestBody)
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

    return device && ((device.type === 'Motyvacinis laiškas' && deviceLetters) || (device.type === 'Loterija' && deviceLotteryParticipants) || (device.type === 'Mainai į kita prietaisą' && deviceOffers) || (device.type === 'Klausimynas' && deviceQuestions_Answers)) ? (
        <div className="my-div" style={{ marginTop: "120px" }}>
           {device.type === 'Mainai į kita prietaisą' && (
    <Container className="home">
        <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Pasiūlymai mainams</h3>
        <Row className="justify-content-center">
            {deviceOffers.map((device) => (
                <Col sm={4} key={device.id} style={{ width: '300px' }}>
                    <Card className="mb-4">
                        <Carousel indicators={false} style={{ height: "250px" }}>
                            {device.images && device.images.map((image, index) => (
                                <Carousel.Item key={index}>
                                    <img
                                        className="d-block w-100"
                                        style={{ objectFit: "cover" }}
                                        src={`data:image/png;base64,${image.data}`}
                                        alt={device.name}
                                    />
                                </Carousel.Item>
                            ))}
                        </Carousel>
                        <Card.Body>
                            <Card.Title>{device.name}</Card.Title>
                            <Card.Text>{device.description}</Card.Text>
                            <Card.Text>{device.message}</Card.Text>
                            <Card.Text>-{device.user}</Card.Text>
                            <ul className="list-group list-group-flush mb-3">
                                <li className="list-group-item d-flex justify-content-between align-items-center">
                                    <span>{device.location}</span>
                                </li>
                            </ul>
                            <div className="d-flex justify-content-between align-items-center">
                                <Button variant="primary" disabled={isSubmitting} onClick={() => handleOfferWinner(device.user, device.name, device.id)}>Mainyti!</Button>
                                <Button variant="primary" onClick={() => navigate(`/klientas/${device.user}`)}>Peržiūrėti profilį</Button>
                            </div>
                        </Card.Body>
                    </Card>
                </Col>
            ))}
        </Row>
    </Container>
)}

            {device.type === 'Motyvacinis laiškas' && (
                <ListGroup>
                    {Object.keys(deviceLetters).length > 0 ? (
                        Object.keys(deviceLetters.letters).map((user) => (
                            <Container key={user}>
                                <Button type="submit" variant="primary" disabled={isSubmitting} onClick={() => handleChosenWinner(device.type, user)}>Atiduoti</Button>
                                <ListGroupItem variant="primary">
    <div className="d-flex justify-content-between align-items-center">
        <div className="d-flex align-items-center">
            <b>Motyvacinis laiškas:</b>&nbsp;{user}
        </div>
        <div className="d-flex align-items-center">
            <Button className='ReviewProfileButton' variant="primary" size="sm" onClick={() => navigate(`/klientas/${user.id}`)}>Peržiūrėti profilį</Button>
        </div>
    </div>
</ListGroupItem>

                                {deviceLetters.letters[user].map((letter, index) => (
                                    <ListGroup key={letter.id}>
                                        <ListGroupItem variant="light"> {letter.letter} </ListGroupItem>
                                    </ListGroup>
                                ))}
                            </Container>
                        ))
                    ) : (
                        <Container>
                            <ListGroupItem>Motyvacinių laiškų nėra.</ListGroupItem>
                        </Container>
                    )}
                </ListGroup>
            )}
            {device.type === 'Loterija' && (
                <ListGroup>
                    <ListGroupItem variant="primary"><b>Loterijos dalyviai</b></ListGroupItem>
                    {deviceLotteryParticipants.length > 0 ? (
                        deviceLotteryParticipants.map((user) => (
                            <ListGroupItem key={user.id} className="d-flex justify-content-between align-items-center">
                                <div className="d-flex align-items-center">
                                    {user.name} {user.surname}
                                </div>
                                <div className="d-flex align-items-center">
                                    <Button className='ReviewProfileButton' variant="primary" size="sm" onClick={() => navigate(`/klientas/${user.id}`)}>Peržiūrėti profilį</Button>
                                </div>
                            </ListGroupItem>
                        ))
                    ) : (
                        <ListGroupItem>Loterijos dalyvių nėra.</ListGroupItem>
                    )}
                </ListGroup>
            )}
            {device.type === 'Klausimynas' && (
                <ListGroup>
                    {Object.keys(deviceQuestions_Answers.questionnaires).length > 0 ? (
                        Object.keys(deviceQuestions_Answers.questionnaires).map((user) => (
                            <Container key={user}>
                                <Button type="submit" variant="primary" disabled={isSubmitting} onClick={() => handleChosenWinner(device.type, user)}>Atiduoti</Button>
                                <ListGroupItem variant="primary">
    <div className="d-flex justify-content-between align-items-center">
        <div className="d-flex align-items-center">
        <b>Klausimyno atsakymai:</b>&nbsp;{user}
        </div>
        <div className="d-flex align-items-center">
            <Button className='ReviewProfileButton' variant="primary" size="sm" onClick={() => navigate(`/klientas/${user.id}`)}>Peržiūrėti profilį</Button>
        </div>
    </div>
</ListGroupItem>

                                {deviceQuestions_Answers.questionnaires[user].map((questionnaire, index) => (
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
        </div>
    ) : (
        <Container className="my-5">
            <div className='outerBoxWrapper d-flex justify-content-center'>
                <Spinner animation="border" role="status" />
            </div>
        </Container>
    );
}