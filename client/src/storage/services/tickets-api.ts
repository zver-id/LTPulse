import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type { ITicket } from "../../components/ticketsScreen/ticketScreen.tsx";

export const ticketsApi = createApi({
    reducerPath: "tickets",
    baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api`}),
    endpoints: (builder) => ({
        getTickets: builder.query<ITicket[], { teamId: number, date: string, ticketType: string}>({
            query: ({teamId, date, ticketType})=>
                `Tickets?teamId=${teamId}&date=${date}&ticketType=${ticketType}`,
        })
    })
});

export const { useGetTicketsQuery } = ticketsApi;