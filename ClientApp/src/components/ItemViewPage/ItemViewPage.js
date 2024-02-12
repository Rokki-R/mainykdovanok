import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Carousel, Col, Container, Row, Form, Button, Card, Spinner } from 'react-bootstrap';
import toast, { Toaster } from 'react-hot-toast';
import './ItemViewPage.css'
import axios from 'axios';

export const ItemViewPage = () => {
    const { itemId } = useParams();
    const [items, setItems] = useState([]);
    const [selectedItem, setSelectedItem] = useState(null);
    const [message, setMessage] = useState('');
    const [answers, setAnswers] = useState({});
    const [item, setItem] = useState(null);
    const [userItems, setUserItems] = useState(null);
    const [viewerId, setViewerId] = useState(null);
    const [currentTime, setCurrentTime] = useState(new Date());
    const [isPastEndTime, setIsPastEndTime] = useState(true);
    const [isUserParticipating, setIsUserParticipating] = useState(false);
    const [isLoggedInAsAdmin, setIsLoggedInAsAdmin] = useState(false);
    const [videoFile, setVideoFile] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchItem = async () => {
            try {
                const response = await axios.get(`api/item/getItem/${itemId}`);
                setItem(response.data);
                setInterval(() => {
                    setCurrentTime(new Date());
                }, 1000);

            } catch (error) {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };

        fetchItem();
    }, [itemId]);

    useEffect(() => {
        if (item) {
            setIsPastEndTime(currentTime.getTime() > new Date(item.endDateTime).getTime());
        }
    }, [item, currentTime]);

    useEffect(() => {
        if (item && item.type === 'Mainai į kita prietaisą') {
            const fetchUserItems = async () => {
                try {
                    const response = await axios.get('api/item/getUserItems');
                    setUserItems(response.data);
                } catch (error) {
                    //toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            };
            fetchUserItems();
        }
        else if (item && item.type === 'Loterija') {
            const fetchIsUserParticipating = async () => {
                try {
                    const response = await axios.get(`api/item/isUserParticipatingInLottery/${itemId}`);
                    setIsUserParticipating(response.data);
                } catch (error) {
                    setIsUserParticipating(false);
                }
            };
            fetchIsUserParticipating();
        }
    }, [item]);

    useEffect(() => {
        const fetchViewerId = async () => {
            try {
                const response = await axios.get('api/user/getCurrentUserId');
                setViewerId(response.data);
            } catch (error) {
                //toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };
        fetchViewerId();
    }, []);

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

    const handleItemSelect = (event) => {
        setSelectedItem(event.target.value);
    };

    const handleMessageChange = (event) => {
        setMessage(event.target.value);
    };

    const handleAnswerChange = (event, questionId) => {
        setAnswers({
            ...answers,
            [questionId]: event.target.value,
        });
    };

    const handleVideoUpload = (event) => {
        const file = event.target.files[0];
        setVideoFile(file);
    };

    const handleSubmit = async (event, isParticipating) => {
        event.preventDefault();

        if (viewerId === null) {
            toast.error('Turite būti prisijungęs!');
            return;
        }

        if (item.type === 'Mainai į kita prietaisą' && !selectedItem) {
            toast.error('Pasirinkite skelbimą, kurį norite pasiūlyti keitimui.');
            return;
        }
        else if (item.type === 'Klausimynas') {
            const unansweredQuestions = item.questions.filter(q => !answers[q.id]);

            if (unansweredQuestions.length > 0) {
                toast.error('Atsakykite į visus klausimus.');
                return;
            }
        }

        const data = {
            selectedItem,
            message,
            ...(item.type === 'Klausimynas' && { answers })
        };

        if (item.type === 'Loterija') {
            if (!isParticipating) {
                axios.post(`api/item/enterLottery/${itemId}`, data)
                    .then(response => {
                        if (response.data) {
                            toast.success('Sėkmingai prisijungėte prie loterijos!');

                            setIsUserParticipating(true);
                            setItem({
                                ...item,
                                participants: item.participants + 1,
                            });
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
            }
            else {
                axios.post(`api/item/leaveLottery/${itemId}`, data)
                    .then(response => {
                        if (response.data) {
                            toast.success('Sėkmingai nebedalyvaujate loterijoje!');

                            setIsUserParticipating(false);
                            setItem({
                                ...item,
                                participants: item.participants - 1,
                            });
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
            }
        }
        else if (item.type === 'Klausimynas') {
            const answersList = Object.entries(data.answers).map(([key, value]) => ({ question: parseInt(key), text: value }));
            console.log(answersList);
            axios.post(`api/item/submitAnswers/${itemId}`, answersList)
                .then(response => {
                    if (response.data) {
                        toast.success('Sėkmingai atsakėte į klausimus!');

                        setIsUserParticipating(true);
                        setItem({
                            ...item,
                            participants: item.participants + 1,
                        });
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
        }
        else if (item.type === 'Mainai į kita prietaisą') {

            const formData = new FormData();
            formData.append('selectedItem', data.selectedItem);
            formData.append('message', data.message);
            axios.post(`api/item/submitOffer/${itemId}`, formData)
                .then(response => {
                    if (response.data) {
                        toast.success('Jūsų pasiūlymas sėkmingai išsiųstas!');

                        setItem({
                            ...item,
                            participants: item.participants + 1,
                        });
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
        }
        else if (item.type === 'Motyvacinis video' && videoFile) {
        const data = new FormData();
        data.append('file', videoFile);

        try {
            const response = await axios.post(`api/item/uploadVideo/${itemId}`, data);

            // Check if the response indicates success
            if (response.data) {
                toast.success('Video successfully uploaded!');
                // Additional success handling if needed
            } else {
                toast.error('Upload failed. Please try again.');
            }
        } catch (error) {
            console.error('Error during video upload:', error);
            toast.error('An error occurred during video upload. Please try again.');
        }
    }
    };


    const handleDelete = async (itemId) => {
        await axios.delete(`/api/item/delete/${itemId}`);
        setItems(items.filter((item) => item.id !== itemId));
        navigate(`/`);
    };

    return item ? (
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
                                <Card.Title>{item.name}</Card.Title>
                                <Card.Text>{item.description}</Card.Text>
                                {item.status !== "Aktyvus" || isPastEndTime ? (
                                    <div>
                                        <hr></hr>
                                        <Card.Text>Šis skelbimas nebegalioja.</Card.Text>
                                    </div>
                                ) : null}
                                <hr></hr>
                                {item.type === 'Mainai į kita prietaisą' && (
                                    <Form onSubmit={handleSubmit}>
                                        <Form.Group>
                                            <Form.Label>Pasirinkite savo prietaisą, kurį norite pasiūlyti:</Form.Label>
                                            <Form.Control as="select" id='selectedItem' value={selectedItem} onChange={(e) => setSelectedItem(e.target.value)} >
                                                <option value="">Pasirinkti skelbimą</option>
                                                {userItems && userItems.filter(item => item.type === `Mainai į kita prietaisą`).map(item => (
                                                    <option key={item.id} value={item.id}>{item.name}</option>
                                                ))}
                                            </Form.Control>
                                        </Form.Group>
                                        <Form.Group>
                                            <Form.Label>Žinutė:</Form.Label>
                                            <Form.Control as="textarea" rows={3} id='message' value={message} onChange={(e) => setMessage(e.target.value)} />
                                        </Form.Group>
                                        <Row>
                                            <Col>
                                                <Button variant="primary" type="submit" disabled={isPastEndTime || item.userId === viewerId}>Siūlyti</Button>
                                            </Col>
                                            <Col className="d-flex justify-content-end">
                                                {isLoggedInAsAdmin || item.userId === viewerId ? (
                                                    <>
                                                        <Button style={{ marginRight: '10px' }} variant="primary" onClick={() => handleDelete(item.id)}>Ištrinti</Button>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/redaguoti/${item.id}`}>
                                                            <Button variant="primary">Redaguoti</Button>
                                                        </Link>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/info/${item.id}`}>
                                                            <Button variant="primary">Siūlymai</Button>
                                                        </Link>
                                                    </>
                                                ) : null}
                                            </Col>
                                        </Row>
                                    </Form>
                                )}
                                {item.type === 'Klausimynas' && (
                                    <Form onSubmit={handleSubmit}>
                                        {item.questions.map((question) => (
                                            <Form.Group key={question.id}>
                                                <Form.Label>{question.question}</Form.Label>
                                                <Form.Control type="text" onChange={(event) => handleAnswerChange(event, question.id)} />
                                            </Form.Group>
                                        ))}
                                        <Row>
                                            <Col>
                                                <Button variant="primary" type="submit" disabled={isPastEndTime || item.userId === viewerId}>Atsakyti</Button>
                                            </Col>
                                            <Col className="d-flex justify-content-end">
                                                {isLoggedInAsAdmin || item.userId === viewerId ? (
                                                    <>
                                                        <Button style={{ marginRight: '10px' }} variant="primary" onClick={() => handleDelete(item.id)}>Ištrinti</Button>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/redaguoti/${item.id}`}>
                                                            <Button variant="primary">Redaguoti</Button>
                                                        </Link>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/info/${item.id}`}>
                                                            <Button variant="primary">Atsakymai</Button>
                                                        </Link>
                                                    </>
                                                ) : null}
                                            </Col>
                                        </Row>
                                    </Form>
                                )}
                                {item.type === 'Loterija' && (
                                    <Form onSubmit={handleSubmit}>
                                        <p>Dalyvių skaičius: {item.participants}</p>
                                        <p>Laimėtojas bus išrinktas {new Date(item.endDateTime).toLocaleString('lt-LT')}</p>
                                        <Row>
                                            <Col>
                                                {isUserParticipating && viewerId !== null ? (
                                                    <Button variant="primary" type="submit" disabled={isPastEndTime || item.userId === viewerId} onClick={(event) => handleSubmit(event, true)}>Nebedalyvauti</Button>
                                                ) : (
                                                    <Button variant="primary" type="submit" disabled={isPastEndTime || item.userId === viewerId} onClick={(event) => handleSubmit(event, false)}>Dalyvauti</Button>
                                                )}
                                            </Col>
                                            <Col className="d-flex justify-content-end">
                                                {isLoggedInAsAdmin || item.userId === viewerId ? (
                                                    <>
                                                        <Button style={{ marginRight: '10px' }} variant="primary" onClick={() => handleDelete(item.id)}>Ištrinti</Button>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/redaguoti/${item.id}`}>
                                                            <Button variant="primary">Redaguoti</Button>
                                                        </Link>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/info/${item.id}`}>
                                                            <Button variant="primary">Dalyviai</Button>
                                                        </Link>
                                                    </>
                                                ) : null}
                                            </Col>
                                        </Row>
                                    </Form>
                                )}
                                {item.type === 'Motyvacinis video' && (
                                <Form onSubmit={handleSubmit}>
                                <Form.Group>
                                <Form.Label>Įkelti motyvacinį video:</Form.Label>
                                <Form.Control type="file" accept="video/*" onChange={handleVideoUpload} />
                                </Form.Group>
                                <Form.Group>
                                
                                </Form.Group>
                                <p>Laimėtojas bus išrinktas {new Date(item.endDateTime).toLocaleString('lt-LT')}</p>
                                <Row>
                                    <Col>
                                        <Button variant="primary" type="submit" disabled={isPastEndTime || item.userId === viewerId}>Įkelti</Button>
                                    </Col>
                                    <Col className="d-flex justify-content-end">
                                        {isLoggedInAsAdmin || item.userId === viewerId ? (
                                            <>
                                                <Button style={{ marginRight: '10px' }} variant="primary" onClick={() => handleDelete(item.id)}>Ištrinti</Button>
                                                <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/redaguoti/${item.id}`}>
                                                    <Button variant="primary">Redaguoti</Button>
                                                </Link>
                                                <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/info/${item.id}`}>
                                                    <Button variant="primary">Dalyviai</Button>
                                                </Link>
                                            </>
                                        ) : null}
                                    </Col>
                                </Row>
                            </Form>
                            )}
                            </Card.Body>
                            <Card.Footer>{item.location} | {new Date(item.creationDateTime).toLocaleString('lt-LT')}</Card.Footer>
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
