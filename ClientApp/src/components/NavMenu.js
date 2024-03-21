import { useState, React, Component } from "react";
import {
  Navbar,
  Nav,
  NavDropdown,
  Form,
  FormControl,
  Button,
  Image,
  Collapse,
} from "react-bootstrap";
import { Link, useNavigate } from "react-router-dom";
import { withRouter } from "./withRouter";
import axios from "axios";
import toast, { Toaster } from "react-hot-toast";
import "./NavMenu.css";

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor(props) {
    super(props);
    this.state = {
      searchQuery: "",
      isClicked: false,
      isLogged: false,
      isLoggedIn: false,
      dropdownOpen: false,
      selectedCategory: "Filtras",
      userProfileImage: "./images/profile.png",
      categories: [],
      devices: [],
      allDevices: false,
    };
    this.handleClick = this.handleClick.bind(this);
  }

  componentDidMount() {
    this.handleLoginClick();
    this.getMyProfileImage();
    this.getCategories();
  }

  getMyProfileImage() {
    axios
      .get("api/user/getMyProfileImage")
      .then((response) => {
        if (response.data.user_profile_image !== undefined) {
          this.setState({
            userProfileImage: response.data.user_profile_image,
          });
        }
      })
      .catch((error) => console.error(error));
  }

  getCategories()
  {
      axios.get("api/device/getCategories")
      .then(response => { this.setState({
          categories : response.data
      })
      })
      .catch(error => {
          console.log(error);
          toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      })
  }

  handleClick() {
    this.setState({
      isClicked: !this.state.isClicked,
    });
  };

  handleSearchInputChange = (event) => {
    this.setState({ searchQuery: event.target.value });
  };
  handleSearch = () => {
    if (!this.state.searchQuery) {
      toast.error("Negalite ieškoti skelbimų nieko neįvedę paieškos lange");
      return;
    }

    axios
      .get("/api/device/search", {
        params: {
          searchWord: this.state.searchQuery,
        }
      })
      .then((response) => {
        this.props.navigate(`/search/${this.state.searchQuery}`, {
          state: {
            searchResults: response.data,
            searchQuery: this.state.searchQuery,
          },
        });
      })
      .catch((error) => {
        console.error(error);
      });
  };

  getDevicesByCategory(categoryId) {
    this.props.navigate(`/search/category/${categoryId}`);
}
getAllDevices = () => {
this.setState({
    allDevices: true
});
this.props.navigate(`/search/category/0`);
};

handleLoginClick = () => {
  this.setState({
    isClicked: !this.state.isClicked,
  });
  
  fetch("api/user/isloggedin/", { method: "GET" })
    .then((response) => {
      if (response.ok) {
        return response.json();
      } else {
        throw new Error("User is not logged in");
      }
    })
    .then((data) => {
      const userRole = data.userRole;
      console.log(userRole)
      this.setState({ isLogged: true, userRole: userRole });
    })
    .catch((error) => {
      console.error("Error checking login status:", error);
    });
};

  handleLogoutClick = () => {
    fetch("api/user/logout", { method: "GET" }).then((response) => {
      if (response.status === 200) {
        // 200 - Ok
        this.setState({ isLogged: false });
        window.location.reload();
        window.location.href = "/prisijungimas";
      } else if (response.status === 401) {
        // 401 - Unauthorized
        toast.error("Jūs jau esate atsijungę!");
      } else {
        // 500 - Internal server error
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
      }
    });
  };

  selectCategory = category => {
    this.setState({
        selectedCategory: category,
    });
  };

  toggleDropdown = () => {
    this.setState((prevState) => ({
      dropdownOpen: !prevState.dropdownOpen,
    }));
  };

  render() {
    const { userProfileImage, userRole } = this.state;
    const ProfileImage =
      userProfileImage.length < 100
        ? userProfileImage
        : `data:image/jpeg;base64,${userProfileImage}`;
    const maxCategoryLength = 20; // Maximum number of characters to display in dropdown toggle

    let displayCategory = this.state.selectedCategory;
    if (displayCategory.length > maxCategoryLength) {
      displayCategory = displayCategory.substring(0, maxCategoryLength) + "...";
    }

    return (
      <>
        <Toaster></Toaster>
        <Navbar
          style={{ backgroundColor: "#3183ab", height: "95px" }}
          expand="lg"
          sticky="top"
        >
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse
            id="basic-navbar-nav"
            style={{ backgroundColor: "#3183ab" }}
          >
            <Form inline className="d-flex">
              <FormControl
                style={{
                  width: "250px",
                  height: "50px",
                  margin: "5px 0px 0px 0px",
                }}
                type="text"
                placeholder="Įveskite..."
                value={this.state.searchQuery}
                onChange={this.handleSearchInputChange}
              />
              <Button
                className="buttonsearch"
                variant="primary"
                onClick={this.handleSearch}
              >
                Ieškoti
              </Button>
            </Form>
            <Nav className="ms-auto">
            <NavDropdown title={displayCategory} className="categories">
                                {this.state.categories.map(category => (
                                    <NavDropdown.Item key={category.id} onClick={() => this.getDevicesByCategory(category.id)}>{category.name}</NavDropdown.Item>
                                ))}
                                <NavDropdown.Divider />
                                <NavDropdown.Item onClick={() => this.getAllDevices()}>Visi</NavDropdown.Item>
                            </NavDropdown>
                            {userRole === 0 && ( // Check if user role is 0 (assuming 0 means the user has the required role)
              <div className="d-inline-block align-middle">
                <Button
                  className="buttoncreate"
                  variant="primary"
                  size="sm"
                  href="/naujasskelbimas"
                >
                  Dovanoti!
                </Button>
              </div>
            )}

              {this.state.isLogged ? (
                <NavDropdown
                  className="custom-dropdown"
                  title={
                    <Image
                      alt="Profilio nuotrauka"
                      src={[ProfileImage]}
                      roundedCircle
                      style={{ height: "75px", width: "75px" }}
                    />
                  }
                  onClick={this.handleLoginClick}
                >
                  <NavDropdown.Item
                    href="/manoprofilis"
                    onClick={this.handleClick}
                  >
                    Profilis
                  </NavDropdown.Item>
                  {userRole === 0 && (
                  <NavDropdown.Item
                    href="/manoskelbimai"
                    onClick={this.handleClick}
                  >
                    Mano skelbimai
                  </NavDropdown.Item>
                  )}
                  <NavDropdown.Item
                    onClick={() => {
                      this.handleLogoutClick();
                    }}
                  >
                    Atsijungti
                  </NavDropdown.Item>
                </NavDropdown>
              ) : (
                <NavDropdown
                  className="custom-dropdown"
                  title={
                    <Image
                      alt="Profilio nuotrauka"
                      src={ProfileImage}
                      roundedCircle
                      style={{ height: "75px", width: "75px" }}
                    />
                  }
                  onClick={this.handleLoginClick}
                >
                  <NavDropdown.Item
                    href="/prisijungimas"
                    onClick={this.handleClick}
                  >
                    Prisijungti
                  </NavDropdown.Item>
                  <NavDropdown.Item
                    href="/registracija"
                    onClick={this.handleClick}
                  >
                    Registruotis
                  </NavDropdown.Item>
                </NavDropdown>
              )}
            </Nav>
          </Navbar.Collapse>
        </Navbar>
      </>
    );
  }
}

export function Redirection(props) {
  const navigate = useNavigate();
  return <NavMenu navigate={navigate}></NavMenu>;
}

export default withRouter(NavMenu);
