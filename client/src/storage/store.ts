import { configureStore } from "@reduxjs/toolkit"
import { teamsApi } from "./services/teams-api.ts"
import {metricsApi} from "./services/metrics-api.ts";

export const store = configureStore({
    reducer: {
        [teamsApi.reducerPath]: teamsApi.reducer,
        [metricsApi.reducerPath]: metricsApi.reducer,
    },
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware()
            .concat(teamsApi.middleware)
            .concat(metricsApi.middleware)
})