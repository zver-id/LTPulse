import {createApi, fetchBaseQuery} from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type {ITicket} from "../../components/ticketsScreen/ticketScreen.tsx";

export const ticketsApi = createApi({
  reducerPath: "tickets",
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/Tickets`}),
  tagTypes: ['tickets', 'ticketsByMetric'],
  endpoints: (builder) => ({

    getTickets: builder.query<ITicket[], { teamId: number, date: string, ticketType: string }>({
      query: ({teamId, date, ticketType}) =>
        `?teamId=${teamId}&date=${date}&ticketType=${ticketType}`,
      providesTags: ['tickets']
    }),

    getTicketByMetric: builder.query<ITicket[], {teamId: number, date: string, metric: string}>({
      query: ({teamId, date, metric}) =>
        `byMetric?teamId=${teamId}&date=${date}&metricType=${metric}`,
      providesTags: ['ticketsByMetric']
    }),

    updateTicket: builder.mutation({
      query: (ticket: ITicket) => ({
        url: '',
        method: 'POST',
        body: ticket
      }),
      invalidatesTags: ['tickets']
    })

  })
});

export const {
  useGetTicketsQuery,
  useGetTicketByMetricQuery,
  useUpdateTicketMutation
} = ticketsApi;
