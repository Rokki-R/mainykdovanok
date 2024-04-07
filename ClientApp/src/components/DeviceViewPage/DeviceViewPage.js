import React, { useState, useEffect } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import {
  Carousel,
  Col,
  Container,
  Row,
  Form,
  Button,
  Card,
  Spinner,
  Accordion,
} from "react-bootstrap";
import toast, { Toaster } from "react-hot-toast";
import "./DeviceViewPage.css";
import axios from "axios";

export const DeviceViewPage = () => {
  const { deviceId } = useParams();
  const [devices, setDevices] = useState([]);
  const [selectedDevice, setSelectedDevice] = useState(null);
  const [message, setMessage] = useState("");
  const [letter, setLetter] = useState("");
  const [device, setDevice] = useState(null);
  const [userDevices, setUserDevices] = useState(null);
  const [viewerId, setViewerId] = useState(null);
  const [currentTime, setCurrentTime] = useState(new Date());
  const [isUserParticipating, setIsUserParticipating] = useState(false);
  const [deviceOwner, setDeviceOwner] = useState(null);
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState("");
  const [answers, setAnswers] = useState({});
  
  const navigate = useNavigate();

  useEffect(() => {
    const fetchDeviceOwnerInfo = async () => {
      try {
        const response = await axios.get(`api/device/getDeviceOwnerInfo/${deviceId}`);
        const deviceOwnerData = response.data;
        setDeviceOwner(deviceOwnerData);

      } catch (error) {
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      }
    };

    fetchDeviceOwnerInfo();
  }, [deviceId]);

  useEffect(() => {
    const fetchDevice = async () => {
      try {
        const response = await axios.get(`api/device/getDevice/${deviceId}`);
        setDevice(response.data);
        setInterval(() => {
          setCurrentTime(new Date());
        }, 1000);
      } catch (error) {
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      }
    };

    fetchDevice();
  }, [deviceId]);

  useEffect(() => {
    if (device && device.type === "Mainai į kita prietaisą") {
      const fetchUserDevices = async () => {
        try {
          const response = await axios.get("api/device/getUserDevices");
          setUserDevices(response.data);
        } catch (error) {
          //toast.error('Įvyko klaida, susisiekite su administratoriumi!');
        }
      };
      fetchUserDevices();
    } else if (device && device.type === "Loterija") {
      const fetchIsUserParticipating = async () => {
        try {
          const response = await axios.get(
            `api/device/isUserParticipatingInLottery/${deviceId}`
          );
          setIsUserParticipating(response.data);
        } catch (error) {
          setIsUserParticipating(false);
        }
      };
      fetchIsUserParticipating();
    }
  }, [device]);

  useEffect(() => {
    const fetchViewerId = async () => {
      try {
        const response = await axios.get("api/user/getCurrentUserId");
        setViewerId(response.data);
      } catch (error) {
        //toast.error('Įvyko klaida, susisiekite su administratoriumi!');
      }
    };
    fetchViewerId();
  }, []);

  useEffect(() => {
    const fetchComments = async () => {
      try {
        const response = await axios.get(`api/comment/getComments/${deviceId}`);
        setComments(response.data.comments);
      } catch (error) {
        toast.error("Įvyko klaida, nepavyko gauti komentarų!");
      }
    };

    fetchComments();
  }, [deviceId]);

  const handleDeviceSelect = (event) => {
    setSelectedDevice(event.target.value);
  };

  const handleMessageChange = (event) => {
    setMessage(event.target.value);
  };

  const handleLetterChange = (event) => {
    setLetter(event.target.value);
  };

  const handleCommentChange = (event) => {
    setNewComment(event.target.value);
  };

  const handleAnswerChange = (event, questionId) => {
    setAnswers({
        ...answers,
        [questionId]: event.target.value,
    });
};

  const handleSubmitComment = async (event) => {
    event.preventDefault();

    if (viewerId === null) {
      toast.error("Turite būti prisijungęs!");
      return;
    }

    try {
      const response = await axios.post(`api/comment/postComment/${deviceId}`, {
        comment: newComment,
      });
      const newCommentData = response.data;
      setNewComment("");
      toast.success("Komentaras sėkmingai pridėtas!");
      const commentsResponse = await axios.get(
        `api/comment/getComments/${deviceId}`
      );
      setComments(commentsResponse.data.comments);
    } catch (error) {
      if (error.response.status === 401) {
        toast.error("Turite būti prisijungęs!");
      } else if (error.response.status === 403) {
        toast.error("Neturite privilegijų palikti komentarą!");
      } else {
        console.log(error);
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      }
    }
  }

  const handleSubmit = async (event, isParticipating) => {
    event.preventDefault();

    if (viewerId === null) {
      toast.error("Turite būti prisijungęs!");
      return;
    }

    if (device.type === "Mainai į kita prietaisą" && !selectedDevice) {
      toast.error("Pasirinkite skelbimą, kurį norite pasiūlyti keitimui.");
      return;
    } 
    
    else if (device.type === "Motyvacinis laiškas" && letter.length === 0) {
      toast.error("Negalite pateikti tuščio motyvacinio laiško!");
      return;
    }

    else if (device.type === 'Klausimynas') {
      const unansweredQuestions = device.questions.filter(q => !answers[q.id]);

      if (unansweredQuestions.length > 0) {
          toast.error('Atsakykite į visus klausimus.');
          return;
      }
  }

    const data = {
      selectedDevice,
      message,
      ...(device.type === 'Klausimynas' && { answers })
    };

    if (device.type === "Loterija") {
      if (!isParticipating) {
        axios
  .post(`api/device/enterLottery/${deviceId}`, data)
  .then((response) => {
    if (response.data) {
      toast.success("Sėkmingai prisijungėte prie loterijos!");
      setIsUserParticipating(true);
      setDevice({
        ...device,
        participants: device.participants + 1,
      });
    } else {
      toast.error("Įvyko klaida, susisiekite su administratoriumi!");
    }
  })
  .catch((error) => {
    console.log(error.response.status)
    if (error.response.status === 401) {
      toast.error("Turite būti prisijungęs!");
    } else if (error.response.status === 403) {
      toast.error("Neturite privilegijų dalyvauti loterijoje!");
    } else {
      console.log(error)
      toast.error("Įvyko klaida, susisiekite su administratoriumi!");
    }
  });



      } else {
        axios
          .post(`api/device/leaveLottery/${deviceId}`, data)
          .then((response) => {
            if (response.data) {
              toast.success("Sėkmingai nebedalyvaujate loterijoje!");

              setIsUserParticipating(false);
              setDevice({
                ...device,
                participants: device.participants - 1,
              });
            } else {
              toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            }
          })
          .catch((error) => {
            if (error.response.status === 401) {
              toast.error("Turite būti prisijungęs!");
            } else {
              toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            }
          });
      }
    } else if (device.type === "Motyvacinis laiškas") {
      const formData = new FormData();
      formData.append("letter", letter);
      axios
        .post(`api/device/submitLetter/${deviceId}`, formData)
        .then((response) => {
          if (response.data) {
            toast.success("Sėkmingai išsiuntėte motyvacinį laišką");
          } else {
            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
          }
        })
        .catch((error) => {
          if (error.response.status === 401) {
            toast.error("Turite būti prisijungęs!");
            return;
          } 
          else if (error.response.status === 403)
          {
            toast.error("Jūs neturite privilegijų pateikti motyvacinį laišką");
            return;
          }
          else if (error.response.status === 409) {
            toast.error("Jūs jau esate pateikęs laišką šiam skelbimui.");
            return;
          }
          {
            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
          }
        });
    } else if (device.type === "Mainai į kita prietaisą") {
      const formData = new FormData();
      formData.append("selectedDevice", data.selectedDevice);
      formData.append("message", data.message);
      axios
        .post(`api/device/submitOffer/${deviceId}`, formData)
        .then((response) => {
          if (response.data) {
            toast.success("Jūsų pasiūlymas sėkmingai išsiųstas!");

            setDevice({
              ...device,
              participants: device.participants + 1,
            });
          } else {
            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
          }
        })
        .catch((error) => {
          if (error.response.status === 401) {
            toast.error("Turite būti prisijungęs!");
          } 
          else if (error.response.status === 403)
          {
            toast.error("Jūs neturite privilegijų siūlyti elektronikos prietaisą mainais");
            return;
          }
          
          else {
            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
          }
        });
    }
    else if (device.type === 'Klausimynas') {
      const answersList = Object.entries(data.answers).map(([key, value]) => ({ question: parseInt(key), text: value }));
      axios.post(`api/device/submitAnswers/${deviceId}`, answersList)
          .then(response => {
              if (response.data) {
                  toast.success('Sėkmingai atsakėte į klausimus!');

                  setIsUserParticipating(true);
                  setDevice({
                      ...device,
                      participants: device.participants + 1,
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
              else if (error.response.status === 409) {
                toast.error("Jūs jau esate atsakęs į šio skelbimo klausimus.");
                return;
              }
              else if (error.response.status === 403)
          {
            toast.error("Jūs neturite privilegijų atsakyti į skelbimo klausimus");
            return;
          }
              else {
                  toast.error('Įvyko klaida, susisiekite su administratoriumi!');
              }
          });
  }
  };

  const handleDelete = async (deviceId) => {
    await axios.delete(`/api/device/delete/${deviceId}`);
    setDevices(devices.filter((device) => device.id !== deviceId));
    navigate(`/`);
  };

  return device && deviceOwner ? (
    <div className="outerBoxWrapper">
      <Toaster />
      <Container className="my-5">
        <Row>
          <Col md={4}>
            {device.images && device.images.length > 0 && (
              <Carousel>
                {device.images.map((image, index) => (
                  <Carousel.Item key={index}>
                    <img
                      className="d-block w-100"
                      src={`data:image/png;base64,${image.data}`}
                      alt={`Image ${index + 1}`}
                      height="320"
                      style={{ border: "1px solid white" }}
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
                <Accordion
                  defaultActiveKey="1"
                  className="borderless-accordion"
                >
                  <Accordion.Item eventKey="0">
                    <Accordion.Header
                      className="accordion-header-text"
                      style={{ outline: "none" }}
                    >
                      Skelbimo savininko informacija
                    </Accordion.Header>
                    <Accordion.Body>
                      <div className="d-flex justify-content-between align-items-start">
                        <div className="owner-info-1">
                          <p>
                            <strong>Vardas:</strong> {deviceOwner.name}{" "}
                            {deviceOwner.surname}
                          </p>
                          <p>
                            <strong>El. paštas:</strong> {deviceOwner.email}
                          </p>
                        </div>
                        <div className="owner-info-2">
                          <p>
                            <strong>Padovanotų prietaisų kiekis:</strong>{" "}
                            {deviceOwner.devicesGifted}{" "}
                          </p>
                          <p>
                            <strong>Laimėtų prietaisų kiekis:</strong>{" "}
                            {deviceOwner.devicesWon}{" "}
                          </p>
                        </div>
                      </div>
                      <hr></hr>
                    </Accordion.Body>
                  </Accordion.Item>
                </Accordion>

                <Card.Title>{device.name}</Card.Title>
                <Card.Text>{device.description}</Card.Text>
                {device.status !== "Aktyvus" ? (
                  <div>
                    <hr></hr>
                    <Card.Text>Šis skelbimas nebegalioja.</Card.Text>
                  </div>
                ) : null}
                <hr></hr>
                {device.type === "Mainai į kita prietaisą" && (
                  <Form onSubmit={handleSubmit}>
                    <Form.Group>
                      <Form.Label>
                        Pasirinkite savo prietaisą, kurį norite pasiūlyti:
                      </Form.Label>
                      <Form.Control
                        as="select"
                        id="selectedDevice"
                        value={selectedDevice}
                        onChange={(e) => setSelectedDevice(e.target.value)}
                      >
                        <option value="">Pasirinkti skelbimą</option>
                        {userDevices &&
                          userDevices
                            .filter(
                              (device) => device.type === `Mainai į kita prietaisą`
                            )
                            .map((device) => (
                              <option key={device.id} value={device.id}>
                                {device.name}
                              </option>
                            ))}
                      </Form.Control>
                    </Form.Group>
                    <Form.Group>
                      <Form.Label>Žinutė:</Form.Label>
                      <Form.Control
                        as="textarea"
                        rows={3}
                        id="message"
                        value={message}
                        onChange={(e) => setMessage(e.target.value)}
                      />
                    </Form.Group>
                    <Row>
                      <Col>
                        <Button
                          variant="primary"
                          type="submit"
                          disabled={device.status !== "Aktyvus" || device.userId === viewerId}
                        >
                          Siūlyti
                        </Button>
                      </Col>
                      <Col className="d-flex justify-content-end">
                        {device.userId === viewerId ? (
                          <>
                            <Button
                              style={{ marginRight: "10px" }}
                              variant="primary"
                              onClick={() => handleDelete(device.id)}
                            >
                              Ištrinti
                            </Button>
                            <Link
                              style={{ marginRight: "10px", marginTop: "9px" }}
                              to={`/skelbimas/redaguoti/${device.id}`}
                            >
                              <Button variant="primary">Redaguoti</Button>
                            </Link>
                            <Link
                              style={{ marginRight: "10px", marginTop: "9px" }}
                              to={`/skelbimas/info/${device.id}`}
                            >
                              <Button variant="primary">Siūlymai</Button>
                            </Link>
                          </>
                        ) : null}
                      </Col>
                    </Row>
                  </Form>
                )}
                {device.type === "Motyvacinis laiškas" && (
                  <Form onSubmit={handleSubmit}>
                    <Row>
                      <Form.Control
                        as="textarea"
                        rows={3}
                        id="letter"
                        value={letter}
                        onChange={(e) => setLetter(e.target.value)}
                        placeholder="Čia galite parašyti motyvacinį laišką, kuriame galite pagrįsti savo norą laimėti elektronikos prietaisą"
                      />
                      <Col>
                        <Button
                          variant="primary"
                          type="submit"
                          disabled={device.status !== "Aktyvus" || device.userId === viewerId}
                        >
                          Atsakyti
                        </Button>
                      </Col>
                      <Col className="d-flex justify-content-end">
                        {device.userId === viewerId ? (
                          <>
                            <Button
                              style={{ marginRight: "10px" }}
                              variant="primary"
                              onClick={() => handleDelete(device.id)}
                            >
                              Ištrinti
                            </Button>
                            <Link
                              style={{ marginRight: "10px", marginTop: "9px" }}
                              to={`/skelbimas/redaguoti/${device.id}`}
                            >
                              <Button variant="primary">Redaguoti</Button>
                            </Link>
                            <Link
                              style={{ marginRight: "10px", marginTop: "9px" }}
                              to={`/skelbimas/info/${device.id}`}
                            >
                              <Button variant="primary">Atsakymai</Button>
                            </Link>
                          </>
                        ) : null}
                      </Col>
                    </Row>
                  </Form>
                )}
                {device.type === "Loterija" && (
                  <Form onSubmit={handleSubmit}>
                    <p>Dalyvių skaičius: {device.participants}</p>
                    <p>
                      Laimėtojas bus išrinktas{" "}
                      {new Date(device.winnerDrawDateTime).toLocaleString("lt-LT")}
                    </p>
                    <Row>
                      <Col>
                        {isUserParticipating && viewerId !== null ? (
                          <Button
                            variant="primary"
                            type="submit"
                            disabled={device.userId === viewerId}
                            onClick={(event) => handleSubmit(event, true)}
                          >
                            Nebedalyvauti
                          </Button>
                        ) : (
                          <Button
                            variant="primary"
                            type="submit"
                            disabled={device.userId === viewerId}
                            onClick={(event) => handleSubmit(event, false)}
                          >
                            Dalyvauti
                          </Button>
                        )}
                      </Col>
                      <Col className="d-flex justify-content-end">
                        {device.userId === viewerId ? (
                          <>
                            <Button
                              style={{ marginRight: "10px" }}
                              variant="primary"
                              onClick={() => handleDelete(device.id)}
                            >
                              Ištrinti
                            </Button>
                            <Link
                              style={{ marginRight: "10px", marginTop: "9px" }}
                              to={`/skelbimas/redaguoti/${device.id}`}
                            >
                              <Button variant="primary">Redaguoti</Button>
                            </Link>
                            <Link
                              style={{ marginRight: "10px", marginTop: "9px" }}
                              to={`/skelbimas/info/${device.id}`}
                            >
                              <Button variant="primary">Dalyviai</Button>
                            </Link>
                          </>
                        ) : null}
                      </Col>
                    </Row>
                  </Form>
                )}
                {device.type === 'Klausimynas' && (
                                    <Form onSubmit={handleSubmit}>
                                        {device.questions.map((question) => (
                                            <Form.Group key={question.id}>
                                                <Form.Label>{question.question}</Form.Label>
                                                <Form.Control type="text" onChange={(event) => handleAnswerChange(event, question.id)} />
                                            </Form.Group>
                                        ))}
                                        <Row>
                                            <Col>
                                                <Button variant="primary" type="submit" disabled={device.status !== "Aktyvus" || device.userId === viewerId}>Atsakyti</Button>
                                            </Col>
                                            <Col className="d-flex justify-content-end">
                                                {device.userId === viewerId ? (
                                                    <>
                                                        <Button style={{ marginRight: '10px' }} variant="primary" onClick={() => handleDelete(device.id)}>Ištrinti</Button>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/redaguoti/${device.id}`}>
                                                            <Button variant="primary">Redaguoti</Button>
                                                        </Link>
                                                        <Link style={{ marginRight: '10px', marginTop: '9px' }} to={`/skelbimas/info/${device.id}`}>
                                                            <Button variant="primary">Atsakymai</Button>
                                                        </Link>
                                                    </>
                                                ) : null}
                                            </Col>
                                        </Row>
                                    </Form>
                                )}
              </Card.Body>
              <Card.Footer>
                {device.location} |{" "}
                {new Date(device.creationDateTime).toLocaleString("lt-LT")}
              </Card.Footer>
            </Card>

            <Card>
              <Card.Body>
                <Card.Title>Komentarai</Card.Title>
                {comments.length > 0 ? (
                  <ul className="list-group">
                    {comments.map((comment) => (
                      <li
                        key={comment.id}
                        className="list-group-item d-flex justify-content-between align-items-center"
                      >
                        <div>
                          <strong>
                            {comment.userName} {comment.userSurname}
                          </strong>
                          : {comment.comment}
                        </div>
                        <div>
                          {new Date(comment.postDateTime).toLocaleString(
                            "lt-LT"
                          )}
                        </div>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p>Nėra komentarų.</p>
                )}

                <Form onSubmit={handleSubmitComment}>
                  <Form.Group controlId="comment">
                    <Form.Label>Palikite komentarą:</Form.Label>
                    <Form.Control
                      as="textarea"
                      rows={3}
                      value={newComment}
                      onChange={handleCommentChange}
                    />
                  </Form.Group>
                  <Button variant="primary" type="submit">
                    Pridėti komentarą
                  </Button>
                </Form>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </Container>
    </div>
  ) : (
    <Container className="my-5">
      <div className="outerBoxWrapper d-flex justify-content-center">
        <Spinner animation="border" role="status" />
      </div>
    </Container>
  );
};
