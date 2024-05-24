import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import toast, { Toaster } from "react-hot-toast";
import { Spinner, Form, Button, Card, Container } from "react-bootstrap";
import { useParams } from "react-router-dom";
import axios from "axios";
import "./DeviceUpdatePage.css";

function DeviceUpdatePage() {
  const { deviceId } = useParams();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [location, setLocation] = useState("");
  const [viewerId, setViewerId] = useState(null);
  const [category, setCategory] = useState("Pasirinkite kategoriją");
  const [categories, setCategories] = useState([]);
  const [images, setImages] = useState([]);
  const [imagesToDelete, setImagesToDelete] = useState([]);
  const [device, setDevice] = useState(null);
  const [showMessage, setShowMessage] = useState(false);

  const navigate = useNavigate();

  useEffect(() => {
    async function fetchDevice() {
      const response = await axios.get(`/api/device/getDevice/${deviceId}`);
      setDevice(response.data);
      setName(response.data.name);
      setDescription(response.data.description);
      setCategory(response.data.fk_Category);
      setLocation(response.data.location);
    }
    fetchDevice();
  }, [deviceId]);

  useEffect(() => {
    async function fetchData() {
      try {
        const [categoriesResponse] = await Promise.all([
          axios.get("api/device/getCategories"),
        ]);
        setCategories(categoriesResponse.data);
      } catch (error) {
        console.log(error);
        toast.error("Įvyko klaida");
      }
    }
    fetchData();
  }, []);

  useEffect(() => {
    const fetchViewerId = async () => {
      try {
        const response = await axios.get("api/user/getCurrentUserId");
        setViewerId(response.data);
      } catch (error) {
        if (error.response.status === 401) {
          navigate("/prisijungimas");
          toast.error("Turite būti prisijungęs!");
        } else {
          toast.error("Įvyko klaida");
        }
      }
    };
    fetchViewerId();
  }, []);

  if (device && viewerId && device.userId !== viewerId) {
    navigate("/");
    toast.error("Jūs neturite prieigos prie šio skelbimo redagavimo!");
  }

  const handleSubmit = async (event) => {
    try {
      event.preventDefault();
      const data = new FormData();
      data.append("name", name || device.name);
      data.append("description", description || device.description);
      data.append("fk_Category", category || device.fk_Category);
      data.append("location", location || device.location);
      if (device.images.length === 0 && images.length === 0) {
        toast.error("Negalite palikti skelbimo be nuotraukos");
        await new Promise((resolve) => setTimeout(resolve, 1000));
        window.location.reload();
        return;
      }
      if (
        name === "" ||
        description === "" ||
        location === "" ||
        category === "Pasirinkite kategoriją" ||
        category === undefined
      ) {
        toast.error("Užpildykite visus laukus!");
        return;
      }

      if (images.length > 6 || images.length + device.images.length > 6) {
        toast.error("Daugiausiai galite įkelti 6 nuotraukas");
        await new Promise((resolve) => setTimeout(resolve, 1000));
        window.location.reload();
        return;
      }

      for (let i = 0; i < images.length; i++) {
        data.append("images", images[i]);
      }

      for (let i = 0; i < imagesToDelete.length; i++) {
        data.append("imagesToDelete", imagesToDelete[i]);
      }

      await axios.put(`/api/device/update/${deviceId}`, data);
      toast.success("Skelbimas sėkmingai atnaujintas!");
      setName("");
      setDescription("");
      setLocation("");
      setCategory("");
      setImages([]);
      setImagesToDelete([]);
      navigate(`/skelbimas/${deviceId}`);
    } catch (error) {
      console.log(error);
      if (error.response) {
        toast.error(error.response.data);
      } else {
        toast.error("Ivyko klaida, susisiekite su administratoriumi!");
      }
    }
  };

  const removeSelectedImage = (indexToRemove) => {
    setImages((prevImages) =>
      prevImages.filter((_, index) => index !== indexToRemove)
    );
  };

  const getAllImages = () => {
    if (images.length > 0) {
      return images.map((image, index) => {
        const imageUrl = URL.createObjectURL(image);
        return (
          <div
            key={index}
            style={{ position: "relative", display: "inline-block" }}
          >
            <img
              src={imageUrl}
              style={{
                width: "128px",
                height: "auto",
                marginRight: "15px",
                border: "1px solid white",
              }}
              alt={`Selected Image ${index}`}
            />
            <button
              type="button"
              className="btn btn-sm btn-danger remove-image-btn"
              style={{ position: "absolute", top: "5px", right: "5px" }}
              onClick={() => removeSelectedImage(index)}
            >
              X
            </button>
          </div>
        );
      });
    }
  };

  function getExistingImages() {
    return (
      <div className="d-flex flex-wrap">
        {device.images.map((image, index) => (
          <Form.Group
            key={index}
            className="d-inline-block mr-3"
            style={{ width: "128px", marginRight: "10px" }}
          >
            <img
              className="image-preview"
              src={`data:image/png;base64,${image.data}`}
              alt={`Image ${index + 1}`}
              height="320"
              style={{ border: "1px solid white" }}
            />
            <div>
              <Button
                variant="secondary"
                onClick={() => handleDeleteImage(image.id)}
              >
                Pašalinti
              </Button>
            </div>
          </Form.Group>
        ))}
      </div>
    );
  }

  function handleDeleteImage(id) {
    setImagesToDelete((prevImagesToDelete) => [...prevImagesToDelete, id]);
    setShowMessage(true);
    setDevice((prevDevice) => ({
      ...prevDevice,
      images: prevDevice.images.filter((image) => image.id !== id),
    }));
  }

  const getAllCategories = () => {
    try {
      return categories.map((category) => {
        return <option value={category.id}>{category.name}</option>;
      });
    } catch (error) {
      toast.error("Įvyko klaida");
      console.log(error);
    }
  };

  const handleCancel = () => {
    navigate(`/skelbimas/${deviceId}`);
  };

  if (!device || !categories) {
    return (
      <div>
        <Spinner>Loading...</Spinner>
      </div>
    );
  }

  return (
    <Container className="my-5 outerDeviceUpdateBoxWrapper">
      <Card className="custom-card">
        <Toaster />
        <Card.Header className="header d-flex justify-content-between align-items-center">
          <div className="text-center">Skelbimo atnaujinimas</div>
        </Card.Header>
        <Card.Body>
          <Form>
            <Form.Group>{getExistingImages()}</Form.Group>
            <Form.Group className="mb-3 mt-3">
              <Form.Control
                type="file"
                name="images"
                multiple
                accept="image/*"
                onChange={(e) => setImages([...e.target.files])}
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
            <Form.Group className="text-center mt-3">
              <Form.Control
                className="input"
                type="text"
                name="location"
                id="location"
                value={location}
                onChange={(event) => setLocation(event.target.value)}
                placeholder="Gyvenamoji vietovė"
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
            <div className="d-flex justify-content-between">
              <Button onClick={(event) => handleSubmit(event)} type="submit">
                Atnaujinti
              </Button>
              <Button onClick={(event) => handleCancel(event)} type="submit">
                Atšaukti
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
}

export default DeviceUpdatePage;
