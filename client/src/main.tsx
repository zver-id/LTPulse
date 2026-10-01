import {createRoot} from 'react-dom/client'
import './index.css'
import Router from "./components/router/Router.tsx";
import {Provider} from 'react-redux'
import {store} from "./storage/store.ts";
import {ConfigProvider} from 'antd';
import ruRU from 'antd/locale/ru_RU';

const antdTheme = {
  token: {
    colorPrimary: '#0057c8',
    colorPrimaryHover: '#0047a6',
    colorPrimaryActive: '#003a86',
    colorLink: '#0057c8',
    colorBgLayout: '#f4f6f9',
    colorText: '#1f2733',
    colorTextSecondary: '#5b6572',
    colorBorder: '#e2e7ee',
    borderRadius: 10,
    borderRadiusLG: 16,
    fontFamily: '"Segoe UI", system-ui, Avenir, Helvetica, Arial, sans-serif',
  },
};

createRoot(document.getElementById('root')!).render(
  <Provider store={store}>
    <ConfigProvider locale={ruRU} theme={antdTheme}>
      <Router/>
    </ConfigProvider>
  </Provider>,
)
