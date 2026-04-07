interface LoadingOrErrorScreenProps {
    isLoading: boolean;
    isError: boolean;
}

function loadingOrErrorScreen ({ isLoading, isError }: LoadingOrErrorScreenProps) {

    if (isLoading) {
        return(
            <div>
                <video width="30%" autoPlay muted loop>
                    <source src='/videos/cat.mp4' type='video/mp4'/>
                </video>
            </div>);
    }
    if (isError) {
        return(
            <div>
                <h2> Данные не загрузились </h2>
                <video width="30%" autoPlay muted loop>
                    <source src='/videos/cat.mp4' type='video/mp4'/>
                </video>
            </div>);
    }
}

export default loadingOrErrorScreen;