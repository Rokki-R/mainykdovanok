import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import HomePage from "./components/HomePage/HomePage";

import { LoginPage } from "./components/LoginPage/LoginPage";
import RegistrationPage from "./components/RegistrationPage/RegistrationPage";
import VerifyEmailPage from "./components/VerifyEmailPage/VerifyEmailPage";

import ItemCreationPage from "./components/ItemCreationPage/ItemCreationPage";
import { ItemViewPage } from "./components/ItemViewPage/ItemViewPage";
const AppRoutes = [
  {
    index: true,
    element: <HomePage />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/fetch-data',
    element: <FetchData />
  },
  {
    path: '/registracija',
    element: <RegistrationPage />
  },
  {
    path: '/prisijungimas',
    element: <LoginPage />
  },
  {
    path: '/verifyemail',
    element: <VerifyEmailPage />
  },
  {
    path: '/naujasskelbimas',
    element: <ItemCreationPage />
  },
  {
    path: '/skelbimas/:itemId',
    element: <ItemViewPage />
  }
];

export default AppRoutes;
