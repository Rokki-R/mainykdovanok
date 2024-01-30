import { useState, React, Component } from 'react';
import { Navbar, Nav, NavDropdown, Form, FormControl, Button, Image, Collapse } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { withRouter } from './withRouter';
import axios from 'axios';
import toast, { Toaster } from 'react-hot-toast';
import './NavMenu.css';

export class NavMenu extends Component {
    static displayName = NavMenu.name;

    constructor(props) {
        super(props);
        this.state = {
            searchQuery: '',
            isClicked: false,
            isLogged: false,
            isLoggedIn: false, // Assume the user is not logged in by default
            isLoggedInAsAdmin: false,
            dropdownOpen: false,
            selectedCategory: 'Filtras',
            userAvatar: './images/profile.png',
            categories: [],
            items: [],
            allItems: false,
        };
        this.handleClick = this.handleClick.bind(this);
    }

    componentDidMount() {
        this.handleLoginClick();
    }


    handleClick() {
        this.setState({
            isClicked: !this.state.isClicked
        });
    };

    handleLoginClick = () => {
        this.setState({
            isClicked: !this.state.isClicked
        });
        fetch('api/user/isloggedin/0', { method: "GET" })
        .then(response => {
            if (response.status == 200) { // 200 - Ok, we are logged in.
                this.setState({ isLogged: true});
            }
        })
        console.log(this.isLogged)
        axios.get('api/user/isloggedin/1')
            .then(response => {
                if (response.status === 200) {
                    this.setState({ isLoggedInAsAdmin: true });
                }
            }
        )
    };

    handleLogoutClick = () => {
        fetch("api/user/logout", { method: "GET" })
        .then(response => {
            if (response.status === 200) { // 200 - Ok
                this.setState({ isLogged: false});
                window.location.reload();
                window.location.href = "/prisijungimas";
            }
            else if (response.status === 401) { // 401 - Unauthorized
                toast.error('Jūs jau esate atsijungę!');
            }
            else { // 500 - Internal server error
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        })
    }

    toggleDropdown = () => {
        this.setState(prevState => ({
            dropdownOpen: !prevState.dropdownOpen,
        }));
    };
    
    render() {
        const { userAvatar } = this.state;
        const avatar = userAvatar.length < 100 ? userAvatar : `data:image/jpeg;base64,${userAvatar}`;
        const maxCategoryLength = 20; // Maximum number of characters to display in dropdown toggle

        let displayCategory = this.state.selectedCategory;
        if (displayCategory.length > maxCategoryLength) {
            displayCategory = displayCategory.substring(0, maxCategoryLength) + '...';
        }

        return (
            <>
                <Toaster></Toaster>
                <Navbar style={{ backgroundColor: '#3183ab', height: '95px' }} expand="lg" sticky="top">
                    <Navbar.Toggle aria-controls="basic-navbar-nav" />
                    <Navbar.Collapse id="basic-navbar-nav" style={{ backgroundColor: '#3183ab' }}>
                        <Form inline className="d-flex">
                            <FormControl style={{ width: '250px', height: '50px', margin: '5px 0px 0px 0px' }} type="text" placeholder="Įveskite..." />
                            <Button className="buttonsearch" variant="primary" >Ieškoti</Button>
                        </Form>
                        <Nav className="ms-auto">
                            <NavDropdown title={displayCategory} className="categories">
                                {this.state.categories.map(category => (
                                    <NavDropdown.Item key={category.id}>{category.name}</NavDropdown.Item>
                                ))}
                                <NavDropdown.Divider />
                                <NavDropdown.Item>Visi</NavDropdown.Item>
                            </NavDropdown>
                            <div className="d-inline-block align-middle">
                                <Button className="buttoncreate" variant="primary" size="sm" href="/skelbimas/naujas">Dovanoti!</Button>
                            </div>

                            {this.state.isLogged ? (
                                <NavDropdown title={<Image alt="Profilio nuotrauka" src={avatar} roundedCircle style={{ height: '75px', width: '75px' }} />} onClick={this.handleLoginClick}>
                                    <NavDropdown.Item href="/profile" onClick={this.handleClick}>Profilis</NavDropdown.Item>
                                    {this.state.isLoggedInAsAdmin ? (
                                        <NavDropdown.Item href="/admin/taisyklos" onClick={this.handleClick}>Taisyklos</NavDropdown.Item>
                                    ) : null}
                                    <NavDropdown.Item  onClick={() => { this.handleLogoutClick() }}>Atsijungti</NavDropdown.Item>
                                </NavDropdown>
                            ) : (
                                <NavDropdown className="custom-dropdown" title={<Image alt="Profilio nuotrauka" src={avatar} roundedCircle style={{ height: '75px', width: '75px' }} />} onClick={this.handleLoginClick}>
                                    <NavDropdown.Item href="/prisijungimas" onClick={this.handleClick}>Prisijungti</NavDropdown.Item>
                                    <NavDropdown.Item href="/registracija" onClick={this.handleClick}>Registruotis</NavDropdown.Item>
                                </NavDropdown>
                            )}
                        </Nav>
                    </Navbar.Collapse>
                </Navbar>
            </>
        );
    }
}

export function Redirection(props)
{
    const navigate = useNavigate();
    return (<NavMenu navigate={navigate}></NavMenu>)
}

export default withRouter(NavMenu)
