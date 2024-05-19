import React, { useEffect, useState } from "react";
import toast, { Toaster } from "react-hot-toast";
import axios from "axios";
import { Form, Button, Card, Container } from "react-bootstrap";
import { useNavigate } from "react-router";
import "./DeviceCreationPage.css";

const DeviceCreationPage = () => {
  const [name, setName] = useState("");
  const [images, setImages] = useState([]);
  const [description, setDescription] = useState("");
  const [location, setLocation] = useState("");
  const [category, setCategory] = useState("Pasirinkite kategoriją");
  const [categories, setCategories] = useState([]);
  const [deviceType, setType] = useState("Pasirinkite, kaip norite atiduoti");
  const [deviceTypes, setDeviceTypes] = useState([]);
  const [lotteryWinnerDrawDate, setLotteryWinnerDrawDate] = useState(null);
  const navigate = useNavigate();

  const questionArray = [
    {
      type: "text",
      id: "1",
      value: "",
    },
  ];
  const [questions, setQuestions] = useState(questionArray);
  const addInput = () => {
    setQuestions((s) => {
      return [
        ...s,
        {
          type: "text",
          value: "",
        },
      ];
    });
  };

  const handleChange = (e) => {
    e.preventDefault();

    const index = e.target.id;
    setQuestions((s) => {
      const newArr = s.slice();
      newArr[index].value = e.target.value;

      return newArr;
    });
  };

  useEffect(() => {
    Promise.all([
      axios.get("api/device/getCategories"),
      axios.get("api/device/getDeviceTypes"),
    ])
      .then(([categoriesResponse, deviceTypesResponse]) => {
        setCategories(categoriesResponse.data);
        setDeviceTypes(deviceTypesResponse.data);
      })
      .catch((error) => {
        console.log(error);
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      });
  }, []);

  const getAllCategories = () => {
    try {
      return categories.map((category) => {
        return <option value={category.id}>{category.name}</option>;
      });
    } catch (error) {
      toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      console.log(error);
    }
  };

  const removeInput = (indexToRemove) => {
    setQuestions((prevQuestions) => {
      return prevQuestions.filter((device, index) => index !== indexToRemove);
    });
  };

  const getAllDeviceTypes = () => {
    try {
      return deviceTypes.map((deviceType) => {
        return <option value={deviceType.id}>{deviceType.name}</option>;
      });
    } catch (error) {
      toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      console.log(error);
    }
  };

  const removeImage = (indexToRemove) => {
    setImages((prevImages) => {
      return prevImages.filter((_, index) => index !== indexToRemove);
    });
  };

  const getAllImages = () => {
    if (images.length > 0) {
      return (
        <div className="image-container">
          {images.map((image, index) => (
            <div key={index}>
              <img
                src={URL.createObjectURL(image)}
                alt={`Image ${index + 1}`}
              />
              <Button
                className="btn btn-danger"
                onClick={() => removeImage(index)}
              >
                -
              </Button>
            </div>
          ))}
        </div>
      );
    }
  };

  function checkFields() {
    console.log(lotteryWinnerDrawDate);
    if (
      name === "" ||
      description === "" ||
      location === "" ||
      category === "Pasirinkite kategoriją" ||
      deviceType === "Pasirinkite, kaip norite atiduoti"
    ) {
      toast.error("Reikia užpildyti visus laukus!");
      return false;
    } else {
      return true;
    }
  }

  const handleCreate = (event) => {
    event.preventDefault();
    if (images.length === 0) {
      toast.error("Privalote įkelti bent vieną nuotrauką", {
        style: {
          backgroundColor: "red",
          color: "white",
        },
      });
      return;
    }
    if (images.length > 6) {
      toast.error("Negalima įkelti daugiau nei 6 nuotraukų!", {
        style: {
          backgroundColor: "red",
          color: "white",
        },
      });
      return;
    }
    if (checkFields()) {
      if (deviceType === "4") {
        let hasEmptyQuestion = false;
        questions.forEach((question) => {
          if (question.value.trim() === "") {
            hasEmptyQuestion = true;
            return;
          }
        });
        if (hasEmptyQuestion) {
          toast.error("Negalite palikti tuščių klausimų!");
          return;
        }
      }
      if (deviceType === "1") {
        if (lotteryWinnerDrawDate === null) {
          toast.error(
            "Privalote pasirinkti loterijos laimėtojo išrinkimo datą!"
          );
          return;
        }
      }
      try {
        const formData = new FormData();
        formData.append("name", name);
        formData.append("description", description);
        formData.append("location", location);
        formData.append("category", category);
        formData.append("type", deviceType);
        formData.append("lotteryWinnerDrawDate", lotteryWinnerDrawDate);
        console.log(lotteryWinnerDrawDate);
        for (let i = 0; i < questions.length; i++) {
          formData.append("questions", questions[i].value);
        }
        for (let i = 0; i < images.length; i++) {
          formData.append("images", images[i]);
        }
        axios
          .post("api/device/create", formData)
          .then((response) => {
            if (response.status === 200) {
              toast.success("Sėkmingai sukūrėtė skelbimą!");
              navigate(`/skelbimas/${response.data}`);
            } else if (response.status === 401) {
              toast.error("Turite būti prisijungęs!");
              navigate("/prisijungimas");
            } else {
              toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            }
          })
          .catch((error) => {
            if (error.response.status === 401) {
              toast.error("Turite būti prisijungęs!");
              navigate("/prisijungimas");
            } else if (error.response.status === 403) {
              toast.error("Jūs neturite privilegijų sukurti skelbimo!");
            } else {
              toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            }
          });
      } catch (error) {
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      }
    }
  };

  const handleCancel = () => {
    navigate("/");
  };

  const today = new Date().toISOString().split("T")[0];

  return (
    <Container className="my-5 outerDeviceCreationBoxWrapper">
      <Card className="custom-card">
        <Toaster />
        <Card.Header className="header d-flex justify-content-between align-items-center">
          <div className="text-center">Skelbimo sukūrimas</div>
        </Card.Header>
        <Card.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Control
                type="file"
                name="images"
                accept="image/*"
                onChange={(e) => {
                  const selectedFiles = Array.from(e.target.files);
                  const invalidFiles = selectedFiles.filter(
                    (file) => !file.type.startsWith("image/")
                  );

                  if (invalidFiles.length > 0) {
                    toast.error("Prašome pasirinkti tik paveikslėlių failus!", {
                      style: {
                        backgroundColor: "red",
                        color: "white",
                      },
                    });
                    // Clear the selected files from the input
                    e.target.value = null;
                    return;
                  }

                  if (!selectedFiles.length) return; // If no file is selected, do nothing

                  const updatedImages = [...images, ...selectedFiles]; // Add the selected files to the current list of images

                  // Check if the number of images exceeds the limit
                  if (updatedImages.length > 6) {
                    toast.error("Negalima įkelti daugiau nei 6 nuotraukų!", {
                      style: {
                        backgroundColor: "red",
                        color: "white",
                      },
                    });
                    return;
                  }

                  // Update the state with the new list of images
                  setImages(updatedImages);
                }}
              />
            </Form.Group>
            <Form.Group>{getAllImages()}</Form.Group>
            <Form.Group className="text-center mt-3">
              <Form.Control
                className="input"
                type="text"
                name="name"
                id="name"
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="Pavadinimas"
              />
            </Form.Group>
            <Form.Group className="text-center form-control-description mb-3">
              <Form.Control
                as="textarea"
                name="description"
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Aprašymas"
              />
            </Form.Group>
            <Form.Group className="text-center">
              <Form.Control
                className="input"
                type="text"
                name="location"
                id="location"
                value={location}
                onChange={(event) => setLocation(event.target.value)}
                placeholder="Gyvenamoji vieta"
              />
            </Form.Group>
            <Form.Group className="text-center mb-3">
              <Form.Select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
              >
                <option>Pasirinkite kategoriją</option>
                {getAllCategories()}
              </Form.Select>
            </Form.Group>
            <Form.Group className="text-center mb-3">
              <Form.Select
                value={deviceType}
                onChange={(e) => setType(e.target.value)}
              >
                <option>Pasirinkite, kaip norite atiduoti</option>
                {getAllDeviceTypes()}
              </Form.Select>
            </Form.Group>
            {deviceType === "4" && (
              <>
                {questions.map((device, i) => {
                  return (
                    <Form.Group
                      className="d-flex align-items-center mb-2"
                      key={i}
                    >
                      <Form.Control
                        onChange={handleChange}
                        value={device.value}
                        id={i.toString()}
                        type={device.type}
                        placeholder="Įrašykite klausimą"
                        className="questionInput"
                      />
                      <div className="mt-2 ml-2">
                        {questions.length - 1 === i && (
                          <Button
                            className="btn btn-primary"
                            onClick={addInput}
                          >
                            +
                          </Button>
                        )}
                      </div>
                      {i > 0 && (
                        <div className="mt-2 ml-2">
                          <Button
                            className="btn btn-danger"
                            onClick={() => removeInput(i)}
                          >
                            -
                          </Button>
                        </div>
                      )}
                    </Form.Group>
                  );
                })}
              </>
            )}
            <Form.Group>
              {deviceType === "1" && (
                <>
                  <Form.Label>
                    Pasirinkite loterijos laimėtojo išrinkimo datą:
                  </Form.Label>
                  <Form.Control
                    type="datetime-local"
                    value={lotteryWinnerDrawDate || ""}
                    onChange={(e) => {
                      const selectedDate = new Date(e.target.value);
                      const currentDate = new Date();
                      if (selectedDate < currentDate) {
                        return;
                      }
                      setLotteryWinnerDrawDate(e.target.value);
                    }}
                    min={today}
                    placeholder="Pasirinkite loterijos laimėtojo išrinkimo datą"
                  />
                </>
              )}
            </Form.Group>
            <div className="d-flex justify-content-between">
              <Button onClick={(event) => handleCreate(event)} type="submit">
                Sukurti
              </Button>
              <Button
                variant="secondary"
                onClick={() => handleCancel()}
                type="button"
              >
                Atšaukti
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
};
export default DeviceCreationPage;
