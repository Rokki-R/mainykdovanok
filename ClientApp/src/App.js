import React, { Component } from 'react';
import { Route, Routes } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import { Layout } from './components/Layout';
import './custom.css';
import SearchResultsByQueryPage from './components/SearchResultsByQueryPage/SearchResultsByQueryPage';
import SearchResultsByCategoryPage from './components/SearchResultsByCategoryPage/SearchResultsByCategoryPage';

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <Layout>
        <Routes>
        <Route path="/search/:searchQuery" element={<SearchResultsByQueryPage />} />
        <Route path="/search/category/:categoryId" element={<SearchResultsByCategoryPage />} />
          {AppRoutes.map((route, index) => {
            const { element, ...rest } = route;
            return <Route key={index} {...rest} element={element} />;
          })}
        </Routes>
      </Layout>
    );
  }
}
