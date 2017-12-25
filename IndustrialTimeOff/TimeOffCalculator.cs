using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Globalization;

namespace IndustrialTimeOff
{
    class TimeOffCalculator
    {
        bool union;
        DateTime hireDate;
        object terminationDate;
        int startingSickBalance;
        int startingVacationBalance;
        int startingFloatingBalance;

        int sickDaysUsed = 0;
        int vacationDaysUsed = 0;
        int floatingDaysUsed = 0;

        List<DateTime> holidays = new List<DateTime>() 
            { 
            DateTime.ParseExact( "01/02/2017", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "05/29/2017", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "07/04/2017", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "09/04/2017", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "11/23/2017", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "11/24/2017", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "12/25/2017", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "01/01/2018", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "05/28/2018", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "07/04/2018", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "09/03/2018", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "11/22/2018", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "11/23/2018", "d", CultureInfo.InvariantCulture ),
            DateTime.ParseExact( "12/25/2018", "d", CultureInfo.InvariantCulture )
            };


        public TimeOffCalculator( int employeeId )
        {
            using ( SqlConnection conn = new SqlConnection( global::IndustrialTimeOff.Properties.Settings.Default.TimeOffDatabaseConnectionString ) )
            {
                conn.Open();

                string employeeQuery = String.Format( "SELECT * FROM Employees where Id = {0}", employeeId );
                //string sql = String.Format( "SELECT Union, HireDate, TerminationDate, StartingSickBalanace, StartingVacationBalanace FROM Employees where Id = {0}", employeeId );

                using ( SqlCommand comm = new SqlCommand( employeeQuery, conn ) )
                {
                    //comm.Parameters.AddWithValue( "@id", employeeId );

                    using ( var reader = comm.ExecuteReader() )
                    {
                        if ( !reader.Read() )
                            throw new Exception( "Something is very wrong" );

                        union = reader.GetBoolean( 2 );
                        hireDate = reader.GetDateTime( 3 );
                        terminationDate = reader.GetValue( 4 );
                        startingVacationBalance = reader.GetInt32( 5 );
                        startingSickBalance = reader.GetInt32( 6 );
                        startingFloatingBalance = reader.GetInt32( 7 );
                    }
                }

                calculateDaysUsed( conn, employeeId );
            }
        }

        public void calculateDaysUsed( SqlConnection conn, int employeeId )
        {
            string timeOffQuery = String.Format( "SELECT * FROM TimeOff where EmployeeId = {0}", employeeId );

            using ( SqlCommand comm = new SqlCommand( timeOffQuery, conn ) )
            {
                using ( var reader = comm.ExecuteReader() )
                {
                    while ( reader.Read() )
                    {
                        string type = reader.GetString( 2 );
                        DateTime start = reader.GetDateTime( 3 );
                        DateTime end = reader.GetDateTime( 4 );

                        switch ( type.TrimEnd(' ') )
                        {
                            case "Sick":
                                if ( union )
                                {
                                    // check to see if date is for this year (10/23 to 10/23)
                                    var unionYearEnd = new DateTime( DateTime.Now.Year, 10, 23 );
                                    if ( DateTime.Now > unionYearEnd )
                                    {
                                        unionYearEnd = unionYearEnd.AddYears( 1 );
                                    }
                                    var unionYearStart = unionYearEnd.AddYears( -1 );
                                    if ( start > unionYearStart && end < unionYearEnd )
                                    {
                                        sickDaysUsed += calculateNumberOfDays( start, end );
                                    }
                                }
                                else // non-union
                                {
                                    if ( start.Year == DateTime.Now.Year )
                                    {
                                        sickDaysUsed += calculateNumberOfDays( start, end );
                                    }
                                }
                                break;
                            case "Vacation":
                                var anniversaryStartDate = new DateTime( DateTime.Now.Year, hireDate.Month, hireDate.Day );
                                if ( anniversaryStartDate > DateTime.Now )
                                {
                                    anniversaryStartDate = anniversaryStartDate.AddYears( -1 );
                                }

                                if ( start > anniversaryStartDate )
                                {
                                    vacationDaysUsed += calculateNumberOfDays( start, end );
                                }
                                break;
                            case "Floating":
                                if ( union )
                                {
                                    var anniversaryStart = new DateTime( DateTime.Now.Year, hireDate.Month, hireDate.Day );
                                    if ( anniversaryStart > DateTime.Now )
                                    {
                                        anniversaryStart = anniversaryStart.AddYears( -1 );
                                    }

                                    if ( start > anniversaryStart )
                                    {
                                        floatingDaysUsed += calculateNumberOfDays( start, end );
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        protected int calculateNumberOfDays( DateTime start, DateTime end )
        {
            int count = 0;
            for ( DateTime index = start; index <= end; index = index.AddDays( 1 ) )
            {
                if ( index.DayOfWeek != DayOfWeek.Sunday && index.DayOfWeek != DayOfWeek.Saturday && !holidays.Contains( index )) 
                    count++;
            }

            return count;
        }

        /*
non-union hired 6/30/16
9/30 - 1 day accrued 
12/30 - 1 day accrued, total of 2
12/31 - 2 days paid out
3/30/17 - 1 day accrued, total of 1
6/30/17 - ?    since this is the one year the remainder of the 6 days will be earned , + 3
9/30/17 - ?  nothing until Jan 1st 
12/30/17 - ? nothing until Jan 1st
12/31/17 - how many days paid out?  If the 4 earned days are not used, then paid out 
1/1/18 - 6 days accrued

union hired 6/30/16
9/30 - 1 day accrued
10/23 - 1 day paid out
12/30 - ?  1  day earned
3/30/17 - ? 1 day earned 
6/30/17 - ? since 1 year, the remainder 3 sick days earned 
9/30/17 - ? nothing until Oct 23rd
10/23/17 - 6 days accrued, how many paid out?   One day was paid out last Oct 23rd, if not taken the remaining 5 days get paid out, with 6 new days earned for the following year. 
         */
        public int CalculateEarnedSickDays()
        {
            int earnedSickDays;

            DateTime endDate = DateTime.Now;
            if ( terminationDate != System.DBNull.Value )
            {
                endDate = (DateTime)terminationDate;
            }

            if ( union )
            {
                var unionYearEnd = new DateTime( DateTime.Now.Year, 10, 23 );
                if ( DateTime.Now < unionYearEnd )
                {
                    unionYearEnd = unionYearEnd.AddYears( -1 );
                }

                // has employee worked a full fiscal year last year (10/23 to 10/23)
                if ( hireDate < unionYearEnd.AddYears( -1 ) )
                {
                    earnedSickDays = 6;
                }
                else
                {
                    //var lastDayOfYear = new DateTime( DateTime.Now.Year - 1, 10, 23 );

                    //var diff = lastDayOfYear - hireDate;
                    var diff = unionYearEnd - hireDate;
                    int daysWorkedPreviousYear = Convert.ToInt32( diff.TotalDays ) - 1;

                    int daysPaid = daysWorkedPreviousYear / 90;

                    if ( daysPaid < 0 )
                        daysPaid = 0;

                    TimeSpan difference = endDate - hireDate;

                    if ( difference.Days < 90 )
                    {
                        earnedSickDays = 0;
                    }
                    else if ( difference.Days < 180 )
                    {
                        earnedSickDays = 1;
                    }
                    else if( difference.Days < 270 )
                    {
                        earnedSickDays = 2;
                    }
                    else if( difference.Days < 365 )
                    {
                        earnedSickDays = 3;
                    }
                    else
                    {
                        earnedSickDays = 6;
                    }

                    earnedSickDays -= daysPaid;
                }
            }
            else
            {
                // if employed entire previous year then 6 days are earned
                if ( hireDate.Year < DateTime.Now.Year - 1 )
                {
                    earnedSickDays = 6;
                }
                else
                {
                    TimeSpan difference = endDate - hireDate;

                    if ( difference.Days < 90 )
                    {
                        earnedSickDays = 0;
                    }
                    else if( difference.Days < 180 )
                    {
                        earnedSickDays = 1;
                    }
                    else if( difference.Days < 270 )
                    {
                        earnedSickDays = 2;
                    }
                    else if( difference.Days < 365 )
                    {
                        earnedSickDays = 3;
                    }
                    else
                    {
                        earnedSickDays = 6;
                    }

                    // now determine how many days have been paid out
                    if ( hireDate.Year != DateTime.Now.Year )
                    {
                        //TimeSpan daysWorkedPreviousYear = DateTime.Now.Year - hireDate;

                        var lastDayOfYear = new DateTime( hireDate.Year, 12, 31 );
                        var diff = lastDayOfYear - hireDate;
                        int daysWorkedPreviousYear = Convert.ToInt32( diff.TotalDays );

                        int daysPaid = daysWorkedPreviousYear / 90;

                        if ( daysPaid > 3 )
                            daysPaid = 3;

                        earnedSickDays -= daysPaid;
                    }
                }
            }
            // only add startingSickBalance for first year
            if ( DateTime.Now.Year == 2017 )
                earnedSickDays += startingSickBalance;

            return earnedSickDays;
        }

        public int CalculateEarnedVacationDays()
        {
            int earnedVacationDays = 0;

            DateTime endDate = DateTime.Now;
            if ( terminationDate != System.DBNull.Value )
            {
                endDate = (DateTime)terminationDate;
            }

            var duration = endDate - hireDate;
            int years = Convert.ToInt32( duration.TotalDays ) / 365;

            if ( years >= 15 )
            {
                earnedVacationDays = 20;
            }
            else if ( years >= 11 )
            {
                earnedVacationDays = years + 5;
            }
            else if ( years >= 8 )
            {
                earnedVacationDays = 15;
            }
            else if ( years >= 2 )
            {
                earnedVacationDays = 10;
            }
            else if ( years >= 1 )
            {
                earnedVacationDays = 5;
            }

            // only add startingSickBalance for first year
            if ( DateTime.Now.Year == 2017 )
                earnedVacationDays += startingVacationBalance;

            return earnedVacationDays;
        }

        public int CalculateEarnedFloatingDays()
        {
            int earnedFloatingDays = 0;

            if ( union )
            {
/*
                var unionYearEnd = new DateTime( DateTime.Now.Year, 10, 23 );
                if ( DateTime.Now < unionYearEnd )
                {
                    unionYearEnd = unionYearEnd.AddYears( -1 );
                }

                // has employee worked a full fiscal year last year (10/23 to 10/23)
                if ( hireDate < unionYearEnd.AddYears( -1 ) )
                {
                    earnedFloatingDays = 3;
                }
*/
/*
// "Did I mention the floating days were not available for the new hires on Oct 23rd?  They should be automatic for all union, even if just hired the day before"
                var unionYearStart = new DateTime( DateTime.Now.Year, 10, 23 );
                if ( DateTime.Now < unionYearStart )
                {
                    unionYearStart = unionYearStart.AddYears( -1 );
                }

                if ( hireDate < unionYearStart )
                {
                    earnedFloatingDays = 3;
                }
*/
                //122117 - decided to simply always return 3  
                earnedFloatingDays = 3;

                // only add startingSickBalance for first year
                if ( DateTime.Now.Year == 2017 )
                    earnedFloatingDays += startingFloatingBalance;
            }

            return earnedFloatingDays;
        }

        public string CalculateSickDays()
        {
            int sickDays = CalculateEarnedSickDays() - sickDaysUsed;

            return sickDays.ToString();
        }

        public string CalculateVacationDays()
        {
            int vacationDays = CalculateEarnedVacationDays() - vacationDaysUsed;

            return vacationDays.ToString();
        }

        public string CalculateFloatingDays()
        {
            int floatingDays = CalculateEarnedFloatingDays() - floatingDaysUsed;

            return floatingDays.ToString();
        }
    }
}
