import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import HomePage from "./components/HomePage/HomePage";

import { LoginPage } from "./components/LoginPage/LoginPage";
import RegistrationPage from "./components/RegistrationPage/RegistrationPage";
import VerifyEmailPage from "./components/VerifyEmailPage/VerifyEmailPage";
import ForgotPasswordPage from "./components/ForgotPasswordPage/ForgotPasswordPage";
import PasswordChangePage from "./components/PasswordChangePage/PasswordChangePage";

import ItemCreationPage from "./components/ItemCreationPage/ItemCreationPage";
import { ItemViewPage } from "./components/ItemViewPage/ItemViewPage";
import { DetailedItemInfoPage } from "./components/DetailedItemInfoPage/DetailedItemInfoPage";
import SearchResultsByQueryPage from "./components/SearchResultsByQueryPage/SearchResultsByQueryPage";
import SearchResultsByCategoryPage from "./components/SearchResultsByCategoryPage/SearchResultsByCategoryPage";

import MyProfilePage from "./components/MyProfilePage/MyProfilePage";
import { UserProfilePage } from "./components/UserProfilePage/UserProfilePage";
import  MyItemsPage from "./components/MyItemsPage/MyItemsPage"
import ItemUpdatePage from './components/ItemUpdatePage/ItemUpdatePage';

import { ItemWinnerPage } from "./components/ItemWinnerPage/ItemWinnerPage";

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
  },
  {
    path: '/skelbimas/info/:itemId',
    element: <DetailedItemInfoPage />
  },
  {
    path: '/manoprofilis',
    element: <MyProfilePage />
  },
  {
    path: '/klientas/:userId',
    element: <UserProfilePage />
  },
  {
    path: '/laimejimas/:itemId',
    element: <ItemWinnerPage />
  },
  {
    path: '/manoskelbimai',
    element: <MyItemsPage />
  },
  {
    path: '/search/:searchQuery',
    element: <SearchResultsByQueryPage />
  },
  {
    path: '/search/category/:categoryId',
    element: <SearchResultsByCategoryPage />
  },
  {
    path: '/pamirsau-slaptazodi',
    element: <ForgotPasswordPage />
  },
  {
    path: '/pakeisti-slaptazodi',
    element: <PasswordChangePage />
  },
  {
    path: '/skelbimas/redaguoti/:itemId',
    element: <ItemUpdatePage />
  },
];

export default AppRoutes;
