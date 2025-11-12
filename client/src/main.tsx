import { createRoot } from 'react-dom/client'
import './index.css'
import Router from "./components/router/Router.tsx";
import {Provider} from 'react-redux'
import {store} from "./storage/store.ts";

createRoot(document.getElementById('root')!).render(
    <Provider store={store}>
        <Router />
    </Provider>,
)
