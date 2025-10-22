using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using CMCS_ST10445830.Models;

namespace claimtest
{
    public class ClaimTests
    {
        //Check if total amount is calculated correctly
        [Fact]
        public void CalculatedTotalAmount()
        {
            //arrange phase 
            var claim = new Claim();

            claim.HoursWorked = 10; //will set hours to 10
            claim.HourlyRate = 45; //set rate to 45

            //act phase

            var getResult = claim.CalculateTotalAmount();

            //assert phase

            Assert.Equal(450, getResult);
        }

        //check if Notes property can be set and retrieved correctly
        [Fact]
        public void AdditionalNotes()
        {
            //arrange phase 
            var claim = new Claim();
            claim.Notes = "This is a test note for the claim.";

            //act phase
            var getResult = claim.Notes;

            //assert phase
            Assert.Equal("This is a test note for the claim.", getResult);
        }

        //Check if file upload property works correctly
        [Fact]
        public void FileUploadProperty()
        {
            //arrange phase 
            var claim = new Claim();

            // Create a mock IFormFile object
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("invoice.pdf");

            //act phase
            claim.DocumentFile = fileMock.Object;
            claim.DocumentUrl = "/uploads/invoice.pdf";

            //assert phase
            Assert.Equal("invoice.pdf", claim.DocumentFile.FileName);
        }

        //Check if status is Approved
        [Fact]
        public void StatusPropertyApproved()
        {
            //arrange phase 
            var claim = new Claim();
            //act phase
            claim.Status = "Approved";
            //assert phase
            Assert.Equal("Approved", claim.Status);
        }

        //Check if default status is "Pending"
        [Fact]
        public void StatusPropertydefault()
        {
            //arrange phase 
            var claim = new Claim();
            //act phase
            var getResult = claim.Status;
            //assert phase
            Assert.Equal("Pending", getResult);
        }

        //Check if status is Rejected
        [Fact]
        public void StatusPropertyRejected()
        {
            //arrange phase 
            var claim = new Claim();
            //act phase
            claim.Status = "Rejected";
            //assert phase
            Assert.Equal("Rejected", claim.Status);
        }
    }
}
