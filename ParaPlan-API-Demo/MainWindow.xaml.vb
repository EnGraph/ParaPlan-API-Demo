﻿Imports System.IO
Imports System.Net
Imports Newtonsoft
Imports Newtonsoft.Json


Class MainWindow

    'ReadOnly api As String = "https://aws.engraph.com/REST/SFCS/"
    ReadOnly api As String = "https://aws.engraph.com/REST/SFCSTEST/"
    Public token As String
    Public kimPW As String = "6637AA99E18177E7663960496752E46A3CCE8012FB8E4D599DF0890EE3C1A299C768CE89FB2C300CB680A1B22FA98E0AB126049EE703AB92B0D8EBEBB99F3CFF"
    Public kimEmail As String = "kim.marsh@st-francis.org"
    Public testEmail As String = "ekidztest@st-francis.org"
    '7YqnrqsMNto7gQ
    Public testPW As String = "EB9376AB5437E2E0469893C69BB32873AC5F20BDB60856CA8D299607626C34D28FC631EAE61FBABF38FFFAD16D331F84F1830A4A99B4B19F3B5AE6E09EA34678"
    Public apiEmail As String = "ekidz@st-francis.org"
    'ba4tBz6tvWwxWa
    Public apiPassword As String = "B84A1ECB990205E8396B458F6C5F6C82C12B2471CA71F794868E787E19000297AA448F74AA721BF7DED6F355644D112C254CFFF1C1675615617057822F9B4FEB"
    Public apiFailuresCounter As Int32 = 0

    Function SearchClientByBrokerID(ByVal searchText As String) As SimpleList(Of Client)
        ValidateToken()

        Dim request As WebRequest = WebRequest.Create(api + $"ClientService/Client/Search/Broker={searchText}?Token={token}&Device=APIDEMO")
        request.ContentType = "application/json; charset=utf-8"
        listResults.Items.Add(request.RequestUri.ToString())
        Dim clients As SimpleList(Of Client) = New SimpleList(Of Client)
        Try
            Using response As HttpWebResponse = request.GetResponse()
                Dim reader As StreamReader = New StreamReader(response.GetResponseStream)
                Dim s As String = reader.ReadToEnd()
                listResults.Items.Add(s)
                clients = JsonConvert.DeserializeObject(Of SimpleList(Of Client))(s)
                If clients.tokenExists = False Or clients.tokenIsValid = False Then
                    apiFailuresCounter = apiFailuresCounter + 1
                    If apiFailuresCounter > 5 Then
                        Throw New Exception("Too many API failures")
                    End If
                    token = GetToken()
                    Return SearchClientByBrokerID(searchText)
                End If
                For Each c In clients.list
                    listResults.Items.Add($"{c.id} {c.name} {c.notes} {DateTime.Parse(c.birthDateJson).ToLongDateString()}")

                Next

            End Using

        Catch ex As Exception
            listResults.Items.Add(ex.ToString())
        End Try

        Return clients

    End Function

    Function PostProcessedTrips()
        ValidateToken()

        Dim request As HttpWebRequest = WebRequest.Create(api + $"TripService/EkidzProcessed?Token={token}&Device=APIDEMO")
        request.ContentType = "application/json; charset=utf-8"
        request.Method = "POST"
        request.KeepAlive = True
        request.Accept = "application/json"

        listResults.Items.Add(request.RequestUri.ToString())

        Dim trips = New List(Of ProcessedTrip)

        Dim t1 = New ProcessedTrip
        t1.tripID = "11240"
        t1.epochStamp = GetEpochTime(DateTime.Now.AddDays(-3)).ToString()

        Dim t2 = New ProcessedTrip
        t2.tripID = "5340"
        t2.epochStamp = GetEpochTime(DateTime.Now.AddDays(-2)).ToString()

        trips.Add(t1)
        trips.Add(t2)

        Dim postJSON = JsonConvert.SerializeObject(trips)


        Using streamWriter As StreamWriter = New StreamWriter(request.GetRequestStream())
            streamWriter.Write(postJSON)

        End Using

        Using response As HttpWebResponse = request.GetResponse()
            Dim reader As StreamReader = New StreamReader(response.GetResponseStream)
            Dim s As String = reader.ReadToEnd()
            listResults.Items.Add(s)
            listResults.Items.Add("Post complete")
        End Using






    End Function

    Function SearchClientByCustomID(ByVal searchText As String) As SimpleList(Of Client)
        ValidateToken()

        Dim request As WebRequest = WebRequest.Create(api + $"ClientService/Client/Search/CustomID={searchText}?Token={token}&Device=APIDEMO")
        request.ContentType = "application/json; charset=utf-8"
        listResults.Items.Add(request.RequestUri.ToString())
        Dim clients As SimpleList(Of Client) = New SimpleList(Of Client)
        Try
            Using response As HttpWebResponse = request.GetResponse()
                Dim reader As StreamReader = New StreamReader(response.GetResponseStream)
                Dim s As String = reader.ReadToEnd()
                listResults.Items.Add(s)
                clients = JsonConvert.DeserializeObject(Of SimpleList(Of Client))(s)
                If clients.tokenExists = False Or clients.tokenIsValid = False Then
                    apiFailuresCounter = apiFailuresCounter + 1
                    If apiFailuresCounter > 5 Then
                        Throw New Exception("Too many API failures")
                    End If
                    token = GetToken()
                    Return SearchClientByCustomID(searchText)
                End If
                For Each c In clients.list
                    listResults.Items.Add($"{c.id} {c.name} {c.notes}")

                Next

            End Using

        Catch ex As Exception
            listResults.Items.Add(ex.ToString())
        End Try

        Return clients

    End Function

    Function GetClientByID(ByVal Id As String) As Client
        ValidateToken()

        Dim request As WebRequest = WebRequest.Create(api + $"ClientService/Client/{Id}?Token={token}&Device=APIDEMO")
        request.ContentType = "application/json; charset=utf-8"
        listResults.Items.Add(request.RequestUri.ToString())
        Dim c As Client = New Client
        Try
            Using response As HttpWebResponse = request.GetResponse()
                Dim reader As StreamReader = New StreamReader(response.GetResponseStream)
                Dim s As String = reader.ReadToEnd()
                listResults.Items.Add(s)
                c = JsonConvert.DeserializeObject(Of Client)(s)
                If c.tokenExists = False Or c.tokenIsValid = False Then
                    apiFailuresCounter = apiFailuresCounter + 1
                    If apiFailuresCounter > 5 Then
                        Throw New Exception("Too many API failures")
                    End If
                    token = GetToken()
                    Return GetClientByID(Id)
                End If
                listResults.Items.Add($"{c.id} {c.name} {c.notes}")

            End Using

        Catch ex As Exception
            listResults.Items.Add(ex.ToString())
        End Try

        Return c
    End Function


    Sub ValidateToken()
        If String.IsNullOrWhiteSpace(token) Then
            token = GetToken()
        End If
    End Sub

    Function GetTrips() As List(Of TripWrapper)

        ValidateToken()

        '/EkidzTrips?Token={token}&Device={device}&DateStart={dateStart}&DateEnd={dateEnd}&HideCancelled={hideCancelled}&hideNoShow={hideNoShow}&Programs={programs}//
        'Dim request As WebRequest = WebRequest.Create(api + $"TripService/Trips?Token={token}&Device=APIDEMO&Driver=31&Date=2016-08-15")
        'Dim request As WebRequest = WebRequest.Create(api + $"TripService/EkidzTrips?Token={token}&Device=APIDEMO&DateStart=2016-08-15&DateEnd=2016-08-15&HideCancelled=0&HideNoShow=0&Programs=14&ClientType=All")
        Dim request As WebRequest = WebRequest.Create(api + $"TripService/EkidzTrips?Token={token}&Device=APIDEMO&DateStart=2016-07-01&DateEnd=2016-07-03&HideCancelled=0&HideNoShow=0&HideNonCompleted=0&Programs=14&ClientType=All&HideProcessed=no")

        request.ContentType = "application/json; charset=utf-8"
        listResults.Items.Add(request.RequestUri.ToString())
        Dim trips As List(Of TripWrapper) = New List(Of TripWrapper)
        Try
            Using response As HttpWebResponse = request.GetResponse()
                Dim reader As StreamReader = New StreamReader(response.GetResponseStream)
                Dim s As String = reader.ReadToEnd()
                listResults.Items.Add(s)
                Dim jsonSettings = New JsonSerializerSettings()
                jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc
                trips = JsonConvert.DeserializeObject(Of List(Of TripWrapper))(s, jsonSettings)
                If trips.First.tokenExists = False Or trips.First.tokenIsValid = False Then
                    apiFailuresCounter = apiFailuresCounter + 1
                    If apiFailuresCounter > 5 Then
                        Throw New Exception("Too many API failures")
                    End If
                    token = GetToken()
                    Return GetTrips()
                End If
                listResults.Items.Add($"{trips.Count} trips")
                For Each t In trips
                    listResults.Items.Add($"{t.Trip.Client.Contact.bestPhoneNumber} - {t.Trip.tripDateJson} - {DateTime.Parse(t.Trip.tripDateJson).ToString()} - {t.Trip.tripId}")
                Next


            End Using

        Catch ex As Exception
            listResults.Items.Add(ex.ToString())
        End Try

        Return trips
    End Function

    Function GetToken() As String
        'https://aws.engraph.com/ParaPlanREST/UserService/Login?UserName=kim.marsh@st-francis.org&Password=6637AA99E18177E7663960496752E46A3CCE8012FB8E4D599DF0890EE3C1A299C768CE89FB2C300CB680A1B22FA98E0AB126049EE703AB92B0D8EBEBB99F3CFF&Device=APIDEMO&Version=0.1&DeviceToken=
        'Dim urlString = $"https://aws.engraph.com/ParaPlanREST/UserService/Login?UserName={kimEmail}&Password={kimPW}&Device=APIDEMO&Version=0.1&DeviceToken="
        Dim urlString = $"https://aws.engraph.com/ParaPlanREST/UserService/Login?UserName={testEmail}&Password={testPW}&Device=APIDEMO&Version=0.1&DeviceToken="

        listResults.Items.Add(urlString)
        Dim request As WebRequest = WebRequest.Create(urlString)
        request.ContentType = "application/json; charset=utf-8"
        Dim rv As String = ""
        Try
            Using response As HttpWebResponse = request.GetResponse()
                Dim reader As StreamReader = New StreamReader(response.GetResponseStream)
                Dim s As String = reader.ReadToEnd()
                listResults.Items.Add(s)
                Dim user As User = JsonConvert.DeserializeObject(Of User)(s)

                listResults.Items.Add(user.name)

                rv = user.key


            End Using

        Catch ex As Exception
            listResults.Items.Add(ex.ToString())
        End Try

        Return rv

    End Function

    Private Sub GetTripsButton_Click(sender As Object, e As RoutedEventArgs)
        GetTrips()
    End Sub

    Private Sub SearchByBroker_Click(sender As Object, e As RoutedEventArgs)
        SearchClientByBrokerID(brokerSearchText.Text)
    End Sub

    Private Sub listResults_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Dim text As String = listResults.SelectedItem.ToString()
        Dim fileLocation As String = System.Environment.CurrentDirectory + "//" + Guid.NewGuid().ToString() + ".txt"
        File.WriteAllText(fileLocation, text)
        Process.Start(fileLocation)
    End Sub

    Private Sub SearchByCustom_Click(sender As Object, e As RoutedEventArgs)
        SearchClientByCustomID(customSearchText.Text)
    End Sub


    Private Sub GetByID_Click(sender As Object, e As RoutedEventArgs)
        GetClientByID(getByIDText.Text)
    End Sub

    Function GetEpochTime(ByVal OriginalDate As DateTime) As Double
        Return (OriginalDate.ToUniversalTime() - New DateTime(1970, 1, 1)).TotalSeconds
    End Function

    Private Sub postClick(sender As Object, e As RoutedEventArgs)
        PostProcessedTrips()
    End Sub
End Class

Public Class RESTBase
    Public tokenIsValid As Boolean
    Public tokenExists As Boolean
    Public errorMessage As String
    Public success As Boolean
End Class
Public Class [Stop]
    Inherits RESTBase

    Public tripID As Int32
    Public clientLastName As String
    Public clientFirstName As String

End Class

Public Class Client
    Inherits RESTBase
    Public id As Int32
    Public name As String
    Public notes As String
    Public birthDateJson As String

End Class

Public Class SimpleList(Of T)
    Inherits RESTBase
    Public list As List(Of T)

End Class

Public Class User
    Public name As String
    Public userId As String
    Public key As String

End Class

Public Class TripWrapper
    Inherits RESTBase
    Public Trip As Trip

End Class

Public Class Trip
    Public appointmentType As String
    Public Client As TripClient
    Public tripId As String
    'Public tripDate As DateTime
    Public tripDateJson As String


End Class

Public Class TripClient
    Public birthDateJson As String
    Public Contact As ContactInformation

End Class

Public Class ContactInformation
    Public bestPhoneNumber As String
    Public cellPhone As String
    Public homePhone As String
    Public otherPhone As String

End Class

Public Class ProcessedTrip
    Public tripID As String
    Public epochStamp As String

End Class


